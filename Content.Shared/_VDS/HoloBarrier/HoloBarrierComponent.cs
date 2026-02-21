using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization;

namespace Content.Shared._VDS.HoloBarrier;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HoloBarrierComponent : Component
{
    /// <summary>
    /// If not null, limits the amount of times this component can trigger.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HoloBarrierState State = HoloBarrierState.Enabled;

    [DataField, AutoNetworkedField]
    public float ReflectProb = 1f;

    [DataField, AutoNetworkedField]
    public bool NeedsPower = true;

    [DataField, AutoNetworkedField]
    public float DrawRate = 100f;

    [DataField]
    public string FixtureID = "holo";
}

[Serializable, NetSerializable]
public enum HoloBarrierState : byte
{
    Enabled,
    Hit,
    Disabled
}

[Serializable, NetSerializable]
public enum HoloBarrierStatus : byte
{
    Active,
    Charging,
}

[Serializable, NetSerializable]
public enum HoloBarrierLayers : byte
{
    Machine,
    Barrier,
    Collide,
    Power
}
