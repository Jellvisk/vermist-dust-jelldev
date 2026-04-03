using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Serialization;

namespace Content.Shared._VDS.Physics;

[RegisterComponent]
public sealed partial class ChainComponent : Component
{
    /// <summary>
    /// The name of our group.
    /// </summary>
    [DataField(readOnly: true, required: true)]
    public string ChainId = "chain";

    /// <summary>
    /// Members of this chain group.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> Members = [];

    // as a chained component???
    [DataField]
    public JointType JointType = JointType.Weld;
}
