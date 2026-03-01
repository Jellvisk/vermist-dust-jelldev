using Content.Shared._VDS.Trigger.Components.Conditions;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Trigger;

namespace Content.Shared._VDS.Trigger.Systems;

public sealed partial class TriggerSystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedBatterySystem _batterySystem = default!;

    private void InitializeCondition()
    {
        SubscribeLocalEvent<IsPoweredTriggerConditionComponent, AttemptTriggerEvent>(OnIsPoweredTriggerAttempt);
    }

    private void OnIsPoweredTriggerAttempt(Entity<IsPoweredTriggerConditionComponent> ent, ref AttemptTriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.Keys.Contains(args.Key))
            return;

        if (TryComp<BatteryComponent>(ent, out var battery)
            && _batterySystem.GetCharge((ent.Owner, battery)) > ent.Comp.MinimumCharge)
        {
            args.Cancelled |= !ent.Comp.IsPowered;
            return;
        }

        if (_powerReceiver.IsPowered(ent.Owner))
        {
            args.Cancelled |= !ent.Comp.IsPowered;
            return;
        }
    }
}
