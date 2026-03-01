using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._VDS.HoloBarrier;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(HoloBarrierSystem))]
public sealed partial class HoloBarrierComponent : Component
{
    /// <summary>
    /// </summary>
    [DataField, AutoNetworkedField]
    public HoloBarrierState State = HoloBarrierState.Enabled;

    [DataField]
    public float DrawRate;

    [DataField]
    public float CurrentCharge;

    [DataField]
    public float PriorCharge;

    [DataField]
    public float ReflectProb = 1f;

    [DataField]
    public float ReflectBatteryScalar = 1f;

    [DataField]
    public bool NeedsPower = true;

    [DataField]
    public ProtoId<SinkPortPrototype> OnPort = "On";

    [DataField]
    public ProtoId<SinkPortPrototype> OffPort = "Off";

    [DataField]
    public ProtoId<SinkPortPrototype> TogglePort = "Toggle";

    [DataField]
    public string FixtureID = "holo";

    [Serializable, NetSerializable]
    public enum HoloBarrierVisuals : byte
    {
        Machine,
        Holo
    }

    [Serializable, NetSerializable]
    public enum HoloBarrierMachineVisualState : byte
    {
        Anchored,
        Unanchored,
    }

    [Serializable, NetSerializable]
    public enum HoloBarrierHoloVisualState : byte
    {
        State,
        ChargeLevel,
    }

    [Serializable, NetSerializable]
    [Flags]
    public enum HoloBarrierHoloVisualStates : byte
    {
        None = 0,
        Enabled = 1 << 0,
        Anchored = 1 << 1,
        Charging = 1 << 2,
        Touching = 1 << 3,
    }

    [Serializable, NetSerializable]
    public enum HoloBarrierHoloChargeLevelVisualState : byte
    {
        None,
        Critical,
        Low,
        Medium,
        High,
        Full,
    }
}

// [Serializable, NetSerializable]
// public sealed class HoloBarrierComponentState(HoloBarrierState state) : ComponentState
// {
//     public HoloBarrierState State = state;
// }

[Flags]
[Serializable, NetSerializable]
public enum HoloBarrierState : byte
{
    None = 0,
    Enabled = 1 << 0,
    Anchored = 1 << 1,
    Charging = 1 << 2,
    HasPower = 1 << 3,
}
