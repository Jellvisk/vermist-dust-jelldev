using Content.Shared.Weapons.Reflect;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Can this entity be reflected.
/// Only applies if it is shot like a projectile and not if it is thrown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class ReflectiveComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("reflective")]
    public ReflectType Reflective = ReflectType.NonEnergy;

    /// <summary>
    /// VDS - Optional cooldown on reflective physic changes.
    /// This helps prevent the projectile from getting stuck in place if it hits two reflective entities at the same time.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// VDS - When we can be affected by reflect physics again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool CanReflectCollideTrigger = true;
}
