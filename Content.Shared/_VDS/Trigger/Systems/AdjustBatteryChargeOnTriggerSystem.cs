using System.Diagnostics.CodeAnalysis;
using Content.Shared._VDS.Trigger.Components.Effects;
using Content.Shared.Damage.Systems;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Trigger;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._VDS.Trigger.Systems;

/// <inheritdoc cref="AdjustBatteryChargeOnTriggerComponent"/>
public sealed partial class AdjustBatteryChargeOnTriggerSystem : XOnTriggerSystem<AdjustBatteryChargeOnTriggerComponent>
{
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdjustBatteryChargeOnTriggerComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<AdjustBatteryChargeOnTriggerComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<AdjustBatteryChargeOnTriggerComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.CollisionKineticPowerScalar == null
            || args.OtherBody.Mass == 0 // no point if there's no mass...
            || ent.Comp.TimeSinceCollision == _timing.CurTime)
            return;

        var velocity = args.OtherBody.LinearVelocity.LengthSquared();

        if (ent.Comp.MinimumCollisionSpeed > velocity)
            return;

        ent.Comp.StoredKineticEnergy = args.OtherBody.Mass * velocity;
        ent.Comp.TimeSinceCollision = _timing.CurTime;
        Log.Info($"gaming collide kinetic style {ent.Comp.StoredKineticEnergy}");
        Log.Info($"gaming collide time style {ent.Comp.TimeSinceCollision}");
        DirtyFields(
            ent.Owner,
            ent.Comp,
            null,
            nameof(AdjustBatteryChargeOnTriggerComponent.StoredKineticEnergy),
            nameof(AdjustBatteryChargeOnTriggerComponent.TimeSinceCollision));
    }

    private void OnDamageChanged(Entity<AdjustBatteryChargeOnTriggerComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.DamagePowerScalar == null
            || args.DamageDelta == null
            || args.DamageDelta.GetTotal() < ent.Comp.MinimumDamageThreshold
            || ent.Comp.TimeSinceDamage == _timing.CurTime)
            return;

        ent.Comp.StoredDamageDeltaTotal = args.DamageDelta.GetTotal();
        ent.Comp.TimeSinceDamage = _timing.CurTime;
        Log.Info($"gaming damage total style {ent.Comp.StoredDamageDeltaTotal}");
        Log.Info($"gaming dmg time style {ent.Comp.TimeSinceDamage}");
        DirtyFields(
            ent.Owner,
            ent.Comp,
            null,
            nameof(AdjustBatteryChargeOnTriggerComponent.StoredDamageDeltaTotal),
            nameof(AdjustBatteryChargeOnTriggerComponent.TimeSinceDamage));
    }

    protected override void OnTrigger(Entity<AdjustBatteryChargeOnTriggerComponent> ent, EntityUid target, ref TriggerEvent args)
    {

        Log.Info($"we are actually winneing");
        if (!TryComp<BatteryComponent>(target, out var battery))
            return;


        var priorCharge = _battery.GetCharge((target, battery));
        var currentCharge = priorCharge;
        var amount = ent.Comp.PowerAdjustment;

        amount += CalculateDamageScalar(ent);
        amount += CalculateKineticScalar(ent);

        var beforeEv = new BeforeAdjustBatteryChargeOnTriggerEvent(
            amount,
            currentCharge,
            target,
            args.Key);
        RaiseLocalEvent(ent.Owner, ref beforeEv);
        Log.Info($"we are so winning with {amount}");

        if (beforeEv.Cancelled)
            return;

        // are we removing charge or adding charge?
        switch (amount)
        {
            case < 0f:
                if (currentCharge <= 0f)
                    break; // nada to do

                // reverse power adjustment because usecharge expects positive numbers
                _battery.UseCharge((target, battery), -amount);
                currentCharge = _battery.GetCharge((target, battery));

                break;
            case > 0f:
                if (_battery.IsFull((target, battery)))
                    break; // nothing to do

                _battery.ChangeCharge((target, battery), amount);
                currentCharge = _battery.GetCharge((target, battery));

                break;
            default: // aka 0. no real point to do anything if we're not adjusting...
                break;
        }

        var afterEv = new AfterAdjustBatteryChargeOnTriggerEvent(
            priorCharge,
            currentCharge,
            target,
            args.Key);
        RaiseLocalEvent(ent.Owner, ref afterEv);

        args.Handled = true;
    }

    private float CalculateDamageScalar(Entity<AdjustBatteryChargeOnTriggerComponent> ent)
    {
        if (ent.Comp.DamagePowerScalar != null
            && ent.Comp.StoredDamageDeltaTotal != null
            && ent.Comp.TimeSinceDamage + ent.Comp.CheckThreshold >= _timing.CurTime)
        {
            return (float)ent.Comp.StoredDamageDeltaTotal.Value * ent.Comp.DamagePowerScalar.Value;
        }

        return 0;
    }

    private float CalculateKineticScalar(Entity<AdjustBatteryChargeOnTriggerComponent> ent)
    {
        if (ent.Comp.CollisionKineticPowerScalar != null
            && ent.Comp.StoredKineticEnergy != null
            && ent.Comp.TimeSinceCollision + ent.Comp.CheckThreshold >= _timing.CurTime)
        {
            return (float)ent.Comp.StoredKineticEnergy.Value * ent.Comp.CollisionKineticPowerScalar.Value;
        }

        return 0;
    }

}

/// <summary>
/// Raised on an entity before it adjusts battery charge with <see cref="AdjustBatteryChargeOnTriggerComponent"/>
/// Used to modify the amount of charge that'll be adjusted.
/// </summary>
[ByRefEvent]
public record struct BeforeAdjustBatteryChargeOnTriggerEvent(float PowerAdjustment, float CurrentCharge, EntityUid Tripper, string? Key = null, bool Cancelled = false);

/// <summary>
/// Raised on an entity after it adjusted its battery charge with <see cref="AdjustBatteryChargeOnTriggerComponent"/>
/// Used for other systems to react to the trigger.
/// </summary>
[ByRefEvent]
public record struct AfterAdjustBatteryChargeOnTriggerEvent(float StoredCharge, float CurrentCharge, EntityUid Tripper, string? Key = null);
