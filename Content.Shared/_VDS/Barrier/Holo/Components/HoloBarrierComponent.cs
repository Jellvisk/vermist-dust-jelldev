namespace Content.Shared._VDS.Barrier;

[RegisterComponent]
public sealed partial class HoloBarrierComponent : Component
{
    [DataField]
    public bool RequiresController = true;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Controller;

}
