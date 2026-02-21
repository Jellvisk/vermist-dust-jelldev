using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;

namespace Content.Shared._VDS.Trigger.Components.Triggers;

/// <summary>
/// Triggers when hurt.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnHurtComponent : BaseTriggerOnXComponent
{
    /// <summary>
    /// If true, the "user" of the trigger is the entity who hit this entity.
    /// If false, the "user" is the trigger.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AssailantIsUser;
}
