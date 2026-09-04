using Content.Shared._VDS.Barrier.Pulveris.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._VDS.Barrier;

[RegisterComponent, NetworkedComponent]
public sealed partial class PulverisBarrierControllerComponent : Component
{ }

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

