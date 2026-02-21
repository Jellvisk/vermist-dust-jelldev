using System.Diagnostics.CodeAnalysis;
using Content.Shared.Physics;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._VDS.HoloBarrier;

///<summary>
///
///</summary>
public sealed class HoloBarrierSystem : EntitySystem
{
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly FixtureSystem _fixture = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HoloBarrierComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);

        SubscribeLocalEvent<HoloBarrierComponent, RefreshChargeRateEvent>(OnRefreshChargeRateEvent);
        SubscribeLocalEvent<HoloBarrierComponent, ComponentStartup>(OnHoloStartup);
        SubscribeLocalEvent<HoloBarrierComponent, ComponentRemove>(OnHoloRemove);
    }

    private void OnHoloRemove(Entity<HoloBarrierComponent> ent, ref ComponentRemove args)
    {
        UpdateRefresh(ent);
    }

    private void OnHoloStartup(Entity<HoloBarrierComponent> ent, ref ComponentStartup args)
    {
        UpdateRefresh(ent);
    }

    private void OnBatteryStateChanged(Entity<HoloBarrierComponent> ent, ref BatteryStateChangedEvent args)
    {
        if (!TryGetOrCreateHoloFixture(ent, out var holoFixture))
        {
            DebugTools.AssertNotNull(holoFixture, $"Could not get or create holographic fixture for {ToPrettyString(ent)}");
            return;
        }

        if (ent.Comp.State != HoloBarrierState.Enabled
            && args.NewState == (BatteryState.Neither | BatteryState.Full))
        {
            // we have power, so make us reflect projectiles and make us hard
            EnsureComp<ReflectComponent>(ent, out var reflect);
            reflect.ReflectProb = ent.Comp.ReflectProb;
            Dirty<ReflectComponent>((ent.Owner, reflect));


            _physics.SetHard(ent, holoFixture, true);

            ent.Comp.State = HoloBarrierState.Enabled;
            Dirty(ent);
        }
        else
        {
            // remove our reflection capabilities and unharden our fixture
            _physics.SetHard(ent, holoFixture, false);
            RemCompDeferred<ReflectComponent>(ent);

            ent.Comp.State = HoloBarrierState.Disabled;
            Dirty(ent);
        }
    }

    private void OnRefreshChargeRateEvent(Entity<HoloBarrierComponent> ent, ref RefreshChargeRateEvent args)
    {
        // UpdateInternalBattery(ent, ref args);
        if (ent.Comp.NeedsPower)
            args.NewChargeRate -= ent.Comp.DrawRate;
    }

    private void UpdateInternalBattery(
        Entity<HoloBarrierComponent> ent,
        ref RefreshChargeRateEvent args,
        BatteryComponent? battery = null)
    {
        if (!Resolve(ent, ref battery, true))
            return;

    }

    public void UpdateRefresh(Entity<HoloBarrierComponent> ent)
    {
        if (ent.Comp.NeedsPower)
            _battery.RefreshChargeRate(ent.Owner);
    }


    private bool TryGetOrCreateHoloFixture(
        Entity<HoloBarrierComponent> ent,
        [NotNullWhen(true)] out Fixture? holoFixture)
    {
        var fixture = _fixture.GetFixtureOrNull(ent, ent.Comp.FixtureID);

        if (fixture == null)
        {
            // no fixture exists, so let's make one.
            _fixture.TryCreateFixture(ent, new PhysShapeAabb(), ent.Comp.FixtureID, 500, true, (int)CollisionGroup.SpecialWallLayer, (int)CollisionGroup.FullTileMask);
            holoFixture = _fixture.GetFixtureOrNull(ent, ent.Comp.FixtureID);

        }
        else
        {
            holoFixture = fixture;
        }


        if (holoFixture == null)
            return false;

        return true;
    }
}
