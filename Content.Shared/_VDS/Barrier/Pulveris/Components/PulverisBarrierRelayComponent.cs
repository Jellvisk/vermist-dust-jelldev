using Content.Shared.NodeContainer;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._VDS.Barrier.Pulveris.Components;

/// <summary>
/// Component for <see cref="PulverisBarrierComponent"/> relays.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PulverisBarrierRelayComponent : Component
{
    /// <summary>
    /// Whether this relay is connected or not.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Connected;

    /// <summary>
    /// Whether this relay is valid at the moment.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Valid = true;

    /// <summary>
    /// Node group ID this relay belongs to.
    /// </summary>
    /// <seealso cref="NodeContainerComponent"/>
    [DataField]
    public string NodeGroupId = "barrier";
}

/// <summary>
/// Keys for appearance data.
/// </summary>
[Serializable, NetSerializable]
public enum PulverisBarrierRelayVisuals : byte
{
    /// <summary>
    /// Current connection state of the relay.
    /// </summary>
    ConnectionVisualState,
    ConnectionVisualActivatingState
}

[Serializable, NetSerializable, Flags]
public enum ConnectionVisualDir : byte
{
    None = 0,
    South = 1 << 0,
    East = 1 << 1,
    North = 1 << 2,
    West = 1 << 3,
}
