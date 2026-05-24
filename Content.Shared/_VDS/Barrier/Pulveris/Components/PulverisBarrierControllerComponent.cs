using Robust.Shared.Serialization;

namespace Content.Shared._VDS.Barrier;

[RegisterComponent]
[Serializable]
public sealed partial class PulverisBarrierControllerComponent : Component
{
    /// <summary>
    /// The entities that this source has control over.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Members = [];


    [DataField]
    public bool Connected;
}

[Serializable, NetSerializable]
public enum PulverisBarrierControllerVisuals : byte
{
    Connected,
}

[Serializable, NetSerializable]
public enum PulverisBarrierControllerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PulverisBarrierControllerUserInterfaceState : BoundUserInterfaceState
{
}

[Serializable, NetSerializable]
public sealed class PulverisBarrierControllerToggleButtonPressedEvent : BoundUserInterfaceMessage
{
}

