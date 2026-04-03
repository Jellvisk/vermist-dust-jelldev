namespace Content.Shared._VDS.Barrier;

[RegisterComponent]
[Serializable]
public sealed partial class HoloBarrierControllerComponent : Component
{
    /// <summary>
    /// The entities that this source has control over.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<Entity<HoloBarrierComponent>> Members = [];
}
