using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;

namespace Content.Shared._VDS.Trigger.Components.Triggers;

// this prevents a shit ton of duplicated code in the system.
// it allows us to subscribe to local events for each triggeronpower component and refer to
// a singular handler per event for all components at once.
public interface ITriggerOnPowerComponent
{
    string? KeyOut { get; }
    PowerTriggerType PowerTrigger { get; }

    enum PowerTriggerType
    {
        Powered,
        NotPowered,
        Both,
    }
}

/// <summary>
/// Triggers when this entity becomes powered or unpowered.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnPowerChangedComponent : BaseTriggerOnXComponent, ITriggerOnPowerComponent
{
    public ITriggerOnPowerComponent.PowerTriggerType PowerTrigger => ITriggerOnPowerComponent.PowerTriggerType.Both;
    string? ITriggerOnPowerComponent.KeyOut => KeyOut;
}

/// <summary>
/// Triggers when this entity becomes powered.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnPoweredComponent : BaseTriggerOnXComponent, ITriggerOnPowerComponent
{
    public ITriggerOnPowerComponent.PowerTriggerType PowerTrigger => ITriggerOnPowerComponent.PowerTriggerType.Powered;
    string? ITriggerOnPowerComponent.KeyOut => KeyOut;
}

/// <summary>
/// Triggers when this entity loses power.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnUnpoweredComponent : BaseTriggerOnXComponent, ITriggerOnPowerComponent
{
    public ITriggerOnPowerComponent.PowerTriggerType PowerTrigger => ITriggerOnPowerComponent.PowerTriggerType.NotPowered;
    string? ITriggerOnPowerComponent.KeyOut => KeyOut;
}
