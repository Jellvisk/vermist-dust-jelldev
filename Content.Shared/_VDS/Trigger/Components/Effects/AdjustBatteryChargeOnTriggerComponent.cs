using Content.Shared.FixedPoint;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared._VDS.Trigger.Components.Effects;

/// <summary>
/// Tries to adjust the charge the entity's battery, if any
/// If TargetUser is true it will adjust the user's instead.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class AdjustBatteryChargeOnTriggerComponent : BaseXOnTriggerComponent
{
    ///<summary>
    /// How much charge to give/take.
    /// Negative values take away power. Positive gives.
    ///</summary>
    [DataField(required: true), AutoNetworkedField]
    public float PowerAdjustment;

    ///<summary>
    /// If set, adds TotalDamage * DamagePowerScalar to <see cref="PowerAdjustment"/>, if being triggered involves any damage.
    ///</summary>
    [DataField, AutoNetworkedField]
    public float? DamagePowerScalar = null;

    ///<summary>
    /// If set, adds Mass * Velocity * CollisionKineticPowerScalar to <see cref="PowerAdjustment"/>, if being triggered involves collisions.
    ///</summary>
    [DataField, AutoNetworkedField]
    public float? CollisionKineticPowerScalar = null;

    ///<summary>
    /// Optional. The minimum (total) damage required to use <see cref="DamagePowerScalar"/>, if the scalar is set.
    ///</summary>
    [DataField]
    public FixedPoint2? MinimumDamageThreshold = null;

    ///<summary>
    /// Optional. The minimum velocity required to use <see cref="CollisionKineticPowerScalar"/>, if the scalar is set.
    ///</summary>
    [DataField]
    public float MinimumCollisionSpeed = 10f;

    [DataField(customTypeSerializer: typeof(TimespanSerializer))]
    [AutoNetworkedField]
    public TimeSpan TimeSinceDamage;

    [DataField(customTypeSerializer: typeof(TimespanSerializer))]
    [AutoNetworkedField]
    public TimeSpan TimeSinceCollision;

    [DataField]
    [AutoNetworkedField]
    public FixedPoint2? StoredDamageDeltaTotal = null;

    [DataField]
    [AutoNetworkedField]
    public float? StoredKineticEnergy = null;

    [DataField(customTypeSerializer: typeof(TimespanSerializer))]
    public TimeSpan CheckThreshold = TimeSpan.FromMilliseconds(200);
}
