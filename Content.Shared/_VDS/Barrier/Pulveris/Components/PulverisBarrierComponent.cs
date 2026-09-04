using Content.Shared.NodeContainer;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._VDS.Barrier;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class PulverisBarrierComponent : Component
{
    [DataField]
    public bool RequiresRelay = true;

    [DataField]
    public bool RequiresPower = true;

    [DataField]
    public bool RequiresConnections = true;

    [DataField, AutoNetworkedField]
    public bool Valid = true;

    [DataField]
    public int MaximumConnections = 2;

    [DataField]
    public int MinimumConnections = 2;

    [DataField, AutoNetworkedField]
    public int CurrentConnections;

    [DataField]
    public HashSet<EntityUid> Relays = [];

    /// <summary>
    /// Node group ID this barrier belongs to.
    /// </summary>
    /// <seealso cref="NodeContainerComponent"/>
    [DataField]
    public string NodeGroupId = "barrier";
}

[ByRefEvent]
public record struct AttemptPulverisBarrierDeactivateEvent(Entity<PulverisBarrierComponent> Barrier, bool Cancelled = false);

[ByRefEvent]
public record struct RefreshBarrierEvent(Entity<PulverisBarrierComponent> Barrier);
