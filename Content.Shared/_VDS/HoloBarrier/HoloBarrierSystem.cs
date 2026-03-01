using System.Diagnostics.CodeAnalysis;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Emag.Systems;
using Content.Shared.Physics;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Weapons.Reflect;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._VDS.HoloBarrier;

///<summary>
///
///</summary>
public sealed class HoloBarrierSystem : EntitySystem
{
    [Dependency]
    private readonly SharedBatterySystem _battery = default!;

    [Dependency]
    private readonly SharedPhysicsSystem _physics = default!;

    [Dependency]
    private readonly SharedPowerReceiverSystem _powerReceiver = default!;

    [Dependency]
    private readonly FixtureSystem _fixture = default!;

    [Dependency]
    private readonly SharedDeviceLinkSystem _deviceLink = default!;

    [Dependency]
    private readonly SharedAppearanceSystem _appearance = default!;

    [Dependency]
    private readonly EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HoloBarrierComponent, ComponentInit>(OnHoloInit);
        SubscribeLocalEvent<HoloBarrierComponent, ComponentRemove>(OnHoloRemove);
        SubscribeLocalEvent<HoloBarrierComponent, ComponentStartup>(OnHoloStartup);

        SubscribeLocalEvent<HoloBarrierComponent, ToggleBarrierEvent>(OnToggleBarrier);

        SubscribeLocalEvent<HoloBarrierComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<HoloBarrierComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<HoloBarrierComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<HoloBarrierComponent, RefreshChargeRateEvent>(OnRefreshChargeRateEvent);
        SubscribeLocalEvent<HoloBarrierComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnHoloInit(Entity<HoloBarrierComponent> ent, ref ComponentInit args)
    {
        _deviceLink.EnsureSinkPorts(ent, ent.Comp.OnPort, ent.Comp.OffPort, ent.Comp.TogglePort);
    }

    private void OnHoloRemove(Entity<HoloBarrierComponent> ent, ref ComponentRemove args)
    {
        UpdateRefresh(ent);
        // DisableBarrier(ent);
    }

    private void OnHoloStartup(Entity<HoloBarrierComponent> ent, ref ComponentStartup args)
    {
        UpdateRefresh(ent);
        UpdatePowerState(ent, ent.Comp.State);

        // if (ent.Comp.State.HasFlag(HoloBarrierState.Enabled))
        // {
        //     EnableBarrier(ent);
        // }
        // else
        // {
        //     DisableBarrier(ent);
        // }
    }

    private void OnToggleBarrier(Entity<HoloBarrierComponent> ent, ref ToggleBarrierEvent args)
    {
        if (args.Activate)
        {
            EnableBarrier(ent);
        }
        else
        {
            DisableBarrier(ent);
        }
    }

    private void OnBatteryStateChanged(Entity<HoloBarrierComponent> ent, ref BatteryStateChangedEvent args)
    {
        if (IsPowered(ent))
        {
            TryToggleBarrier((ent.Owner, ent.Comp), true);
            return;
        }
        UpdatePowerState(ent, ent.Comp.State);
        TryToggleBarrier((ent.Owner, ent.Comp), false);
    }

    private void OnChargeChanged(Entity<HoloBarrierComponent> ent, ref ChargeChangedEvent args)
    {

        ent.Comp.PriorCharge = ent.Comp.CurrentCharge;
        ent.Comp.CurrentCharge = args.CurrentCharge;

        if (IsCharging(ent))
            UpdatePowerState(ent, ent.Comp.State);
    }

    private void OnEmagged(Entity<HoloBarrierComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(ent, EmagType.Interaction))
            return;

        args.Handled = true;
    }

    private void OnRefreshChargeRateEvent(Entity<HoloBarrierComponent> ent, ref RefreshChargeRateEvent args)
    {
        if (!ent.Comp.NeedsPower)
            return;

        args.NewChargeRate -= ent.Comp.DrawRate;
    }

    private void OnSignalReceived(Entity<HoloBarrierComponent> ent, ref SignalReceivedEvent args)
    {
        UpdatePowerState(ent, ent.Comp.State);

        if (args.Port == ent.Comp.OffPort)
        {
            TryToggleBarrier((ent.Owner, ent.Comp), false);
        }
        else if (args.Port == ent.Comp.OnPort)
        {
            TryToggleBarrier((ent.Owner, ent.Comp), true);
        }
        else if (args.Port == ent.Comp.TogglePort)
        {
            TryToggleBarrier((ent.Owner, ent.Comp), !ent.Comp.State.HasFlag(HoloBarrierState.Enabled));
        }
    }

    private HoloBarrierState GetPowerState(Entity<HoloBarrierComponent> ent, HoloBarrierState state = HoloBarrierState.None)
    {
        state = GetUpdatedPoweredState((ent.Owner, ent.Comp), state);
        state = GetUpdatedChargeState((ent.Owner, ent.Comp), state);

        return state;
    }

    private bool UpdatePowerState(Entity<HoloBarrierComponent> ent, HoloBarrierState state = HoloBarrierState.None)
    {
        var newState = GetPowerState(ent, state);

        // only dirty if there is a change
        if (state == newState)
            return false;

        ent.Comp.State = newState;
        Dirty(ent);

        // UpdateAppearanceData((ent.Owner, ent.Comp));
        return true;
    }

    private void EnableBarrier(Entity<HoloBarrierComponent> ent)
    {
        if (EnsureHoloFixture(ent, out var holoFixture))
            _physics.SetHard(ent, holoFixture, true);

        EnsureComp<ReflectComponent>(ent, out var reflect);

        // if (ent.Comp.ReflectBatteryScalar != 1f) // todo: move to own method
        // {
        //     reflect.ReflectProb = CalculateReflectPercentage(
        //         _battery.GetChargeLevel(ent.Owner),
        //         ent.Comp.ReflectBatteryScalar
        //     );
        // }
        // else
        // {
        //     reflect.ReflectProb = ent.Comp.ReflectProb;
        // }
        // Dirty(ent.Owner, reflect);

        // UpdateAppearanceData(ent.Owner);
    }

    private void DisableBarrier(Entity<HoloBarrierComponent> ent)
    {
        // remove our reflection capabilities and unharden our fixture
        if (EnsureHoloFixture(ent, out var holoFixture))
            _physics.SetHard(ent, holoFixture, false);

        RemCompDeferred<ReflectComponent>(ent);
        // UpdateAppearanceData(ent.Owner);
        return;
    }

    public void UpdateRefresh(Entity<HoloBarrierComponent> ent, BatteryComponent? battery = null)
    {
        if (!ent.Comp.NeedsPower)
            return;

        if (CanToggleBarrier(ent, out var _))
            _battery.RefreshChargeRate((ent.Owner, battery));
    }

    private static bool CanToggleBarrier(Entity<HoloBarrierComponent> ent, out BarrierToggleReason reason)
    {
        if (ent.Comp.NeedsPower == false)
        {
            reason = BarrierToggleReason.Forced;
            return true;
        }

        if (ent.Comp.State.HasFlag(HoloBarrierState.HasPower))
        {
            reason = BarrierToggleReason.Interaction;
            return true;
        }
        else
        {
            reason = BarrierToggleReason.LostPower;
            return false;
        }
    }

    public bool TryToggleBarrier(Entity<HoloBarrierComponent?> ent, bool enable)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (enable == ent.Comp.State.HasFlag(HoloBarrierState.Enabled))
            return false;

        CanToggleBarrier((ent.Owner, ent.Comp), out var reason);

        switch (reason)
        {
            case BarrierToggleReason.Interaction or BarrierToggleReason.Forced:
                ToggleBarrier((ent.Owner, ent.Comp), enable);
                return true;

            case BarrierToggleReason.LostPower:
                ToggleBarrier((ent.Owner, ent.Comp), false);
                return false;

            default:
                return false;
        }
    }

    private void ToggleBarrier(Entity<HoloBarrierComponent> ent, bool enable)
    {
        ent.Comp.State = enable
            ? ent.Comp.State |= HoloBarrierState.Enabled
            : ent.Comp.State &= ~HoloBarrierState.Enabled;
        Dirty(ent);

        var ev = new ToggleBarrierEvent(enable);
        RaiseLocalEvent(ent, ref ev);
    }

    public bool IsPowered(Entity<HoloBarrierComponent> ent, BatteryComponent? battery = null)
    {
        if (!Resolve(ent, ref battery))
            return false;

        if (ent.Comp.CurrentCharge > 0f)
        {
            Log.Info($"getttiiiiing battery time. {battery.State} charge {_battery.GetCharge(ent.Owner)}");
            return true;
        }
        else
        {
            return false;
        }
    }

    [PublicAPI]
    public HoloBarrierState GetUpdatedPoweredState(Entity<HoloBarrierComponent?> ent, HoloBarrierState state = HoloBarrierState.None, BatteryComponent? battery = null)
    {
        if (!Resolve(ent, ref ent.Comp) || !Resolve(ent, ref battery))
            return state;

        state = IsPowered((ent.Owner, ent.Comp), battery) ? state |= HoloBarrierState.HasPower : state &= ~HoloBarrierState.HasPower;

        return state;
    }

    private bool IsCharging(Entity<HoloBarrierComponent> ent, BatteryComponent? battery = null)
    {
        if (!Resolve(ent, ref battery))
            return false;

        if (
            battery.State != BatteryState.Full
            && ent.Comp.State.HasFlag(HoloBarrierState.HasPower)
            && ent.Comp.CurrentCharge > ent.Comp.PriorCharge
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [PublicAPI]
    public HoloBarrierState GetUpdatedChargeState(Entity<HoloBarrierComponent?> ent, HoloBarrierState state = HoloBarrierState.None, BatteryComponent? battery = null)
    {
        if (!Resolve(ent, ref ent.Comp) || !Resolve(ent, ref battery))
            return state;

        state = IsCharging((ent.Owner, ent.Comp), battery) ? state |= HoloBarrierState.Charging : state &= ~HoloBarrierState.Charging;

        return state;
    }

    private bool EnsureHoloFixture(Entity<HoloBarrierComponent> ent, [NotNullWhen(true)] out Fixture? holoFixture)
    {
        holoFixture = _fixture.GetFixtureOrNull(ent, ent.Comp.FixtureID);
        if (holoFixture == null)
        {
            // no fixture exists, so let's make one.
            if (_fixture.TryCreateFixture(
                ent,
                new PhysShapeAabb(),
                ent.Comp.FixtureID,
                500,
                true,
                (int)CollisionGroup.SpecialWallLayer,
                (int)CollisionGroup.FullTileMask))
            {
                holoFixture = _fixture.GetFixtureOrNull(ent, ent.Comp.FixtureID);
                return holoFixture != null;
            }

            return false;
        }

        return true;
    }

    public void UpdateAppearanceData(
        Entity<HoloBarrierComponent?> ent,
        BatteryComponent? battery = null,
        AppearanceComponent? appearance = null
    )
    {
        if (!Resolve(ent, ref ent.Comp, ref appearance))
            return;

        // machine
        _appearance.SetData(
            ent,
            HoloBarrierComponent.HoloBarrierVisuals.Machine,
            ent.Comp.State.HasFlag(HoloBarrierState.Anchored),
            appearance
        );

        // hologram
        if (
            ent.Comp.State.HasFlag(HoloBarrierState.Enabled)
            && (ent.Comp.State.HasFlag(HoloBarrierState.HasPower) || ent.Comp.NeedsPower)
        )
        {
            var holoStates = HoloBarrierComponent.HoloBarrierHoloVisualStates.Enabled;

            if (ent.Comp.State.HasFlag(HoloBarrierState.Anchored))
            {
                holoStates |= HoloBarrierComponent.HoloBarrierHoloVisualStates.Anchored;
            }

            if (ent.Comp.State.HasFlag(HoloBarrierState.Charging))
            {
                holoStates |= HoloBarrierComponent.HoloBarrierHoloVisualStates.Charging;
            }

            if (EnsureHoloFixture((ent.Owner, ent.Comp), out var holoFixture))
            {
                if (holoFixture.Contacts.Count != 0)
                    holoStates |= HoloBarrierComponent.HoloBarrierHoloVisualStates.Touching;
            }

            // lights
            // if (ent.Comp.NeedsPower && Resolve(ent.Owner, ref battery))
            // {
            // }
            //

            _appearance.SetData(ent, HoloBarrierComponent.HoloBarrierVisuals.Holo, holoStates, appearance);
        }
        else
        {
            _appearance.SetData(ent, HoloBarrierComponent.HoloBarrierVisuals.Holo, false, appearance);
        }

        // _appearance.SetData(ent, HoloBarrierComponent.HoloBarrierHoloVisualStates.Anchored, ent.Comp.State.HasFlag(HoloBarrierState.Anchored), appearance);
        // _appearance.SetData(ent, HoloBarrierComponent.HoloBarrierHoloVisualStates.Enabled, ent.Comp.Enabled, appearance);
        // _appearance.SetData(ent, HoloBarrierComponent.HoloBarrierHoloVisualStates.Charging, ent.Comp.IsCharging, appearance);

        // if (EnsureHoloFixture((ent.Owner, ent.Comp), out var holoFixture))
        // {
        //     _appearance.SetData(ent, HoloBarrierComponent.HoloBarrierHoloVisualStates.Touching, holoFixture.Contacts.Count != 0, appearance);
        // }

        // lights
    }

    private static float CalculateReflectPercentage(float batteryPercent, float scalar = 1f)
    {
        return MathHelper.Clamp(batteryPercent * scalar, 0f, 100f);
    }

    public enum BarrierToggleReason
    {
        Interaction,
        LostPower,
        Forced,
    }
}

[ByRefEvent]
public readonly record struct ToggleBarrierEvent(bool Activate);
