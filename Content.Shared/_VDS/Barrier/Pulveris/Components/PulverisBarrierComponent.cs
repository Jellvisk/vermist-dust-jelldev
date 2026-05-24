namespace Content.Shared._VDS.Barrier;

[RegisterComponent]
public sealed partial class PulverisBarrierComponent : Component
{
    [DataField]
    public bool RequiresController = true;

    [DataField]
    public bool RequiresPower = true;

    [DataField]
    public bool RequiresConnections = true;

    [DataField]
    public HashSet<EntityUid> Controllers = [];
}
