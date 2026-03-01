using Content.Shared.Trigger.Components.Conditions;
using Robust.Shared.GameStates;

namespace Content.Shared._VDS.Trigger.Components.Conditions;

/// <summary>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IsPoweredTriggerConditionComponent : BaseTriggerConditionComponent
{
    /// <summary>
    /// True: Allows trigger if there is power.
    /// False: Allows trigger if there is no power.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsPowered = true;

    /// <summary>
    /// If the power is coming from a battery, what is the minimum charge
    /// to allow the trigger?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinimumCharge = 0f;
}
