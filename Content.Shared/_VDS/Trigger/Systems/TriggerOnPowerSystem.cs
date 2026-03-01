using Content.Shared._VDS.Trigger.Components.Triggers;
using Content.Shared.Damage.Systems;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Shared._VDS.Trigger.Systems;

/// <summary>
/// Trigger system for being hurt.
/// </summary>
public sealed class TriggerOnPowerSystem : TriggerOnXSystem
{
    private EntityQuery<BatteryComponent> _batteryQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _batteryQuery = GetEntityQuery<BatteryComponent>();

        SubscribeLocalEvent<TriggerOnPoweredComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<TriggerOnPoweredComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<TriggerOnUnpoweredComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<TriggerOnUnpoweredComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<TriggerOnPowerChangedComponent, BatteryStateChangedEvent>(OnBatteryStateChanged);
        SubscribeLocalEvent<TriggerOnPowerChangedComponent, PowerChangedEvent>(OnPowerChanged);
    }


    private void OnPowerChanged(EntityUid uid, ITriggerOnPowerComponent comp, ref PowerChangedEvent args)
    {
        // prioritize batterystatechanged if possible.
        Log.Info("powa change");
        if (HasBattery(uid))
            return;

        switch (comp.PowerTrigger)
        {
            case ITriggerOnPowerComponent.PowerTriggerType.Powered when args.Powered:
                Trigger.Trigger(uid, uid, comp.KeyOut);
                break;
            case ITriggerOnPowerComponent.PowerTriggerType.NotPowered when !args.Powered:
                Trigger.Trigger(uid, uid, comp.KeyOut);
                break;
            case ITriggerOnPowerComponent.PowerTriggerType.Both:
                Trigger.Trigger(uid, uid, comp.KeyOut);
                break;

        }
    }

    private void OnBatteryStateChanged(EntityUid uid, ITriggerOnPowerComponent comp, ref BatteryStateChangedEvent args)
    {
        switch (comp.PowerTrigger)
        {
            case ITriggerOnPowerComponent.PowerTriggerType.Powered when args.NewState != BatteryState.Empty:
                Trigger.Trigger(uid, uid, comp.KeyOut);
                Log.Info("battery trigger powered");
                break;
            case ITriggerOnPowerComponent.PowerTriggerType.NotPowered when args.NewState == BatteryState.Empty:
                Trigger.Trigger(uid, uid, comp.KeyOut);
                Log.Info("battery trigger no power");
                break;
            case ITriggerOnPowerComponent.PowerTriggerType.Both:
                if (args.NewState == BatteryState.Neither && args.OldState == BatteryState.Full
                    || args.NewState == BatteryState.Full && args.OldState == BatteryState.Neither)
                    break;
                Trigger.Trigger(uid, uid, comp.KeyOut);
                Log.Info("battery trigger toggle");
                break;

        }
    }

    private bool HasBattery(EntityUid uid, BatteryComponent? battery = null)
    {
        return _batteryQuery.Resolve(uid, ref battery);
    }
}
