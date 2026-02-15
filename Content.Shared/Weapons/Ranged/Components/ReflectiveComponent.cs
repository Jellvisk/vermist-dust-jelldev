using Content.Shared.Weapons.Reflect;
using Robust.Shared.GameStates;

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
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? PhysicsCooldown;

    /// <summary>
    /// VDS - When we can be affected by reflect physics again.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan PhysicsCooldownEnd;
}
