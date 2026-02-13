namespace Content.Shared._VDS.Projectiles;

/// <summary>
/// Allows this projectile to always penetrate entities, regardless if the projectile has been tanked or not.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectilePenetrationOverrideComponent : Component
{
    [DataField]
    public int MaxPenetrations = 0;

    [DataField]
    public int CurrentPenetrationCount = 0;

}
