using Robust.Shared.GameStates;

namespace Content.Shared._VDS.Weapons.Reflect;

/// <summary>
/// Overrides <see cref="Shared.Weapons.Reflect.ReflectComponent.ReflectProb"/> to be
/// based on any existing battery charge level.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChargedReflectComponent : Component
{
    /// <summary>
    /// The highest reflect probability allowed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ReflectProbMax = 1f;

    /// <summary>
    /// The lowest reflect probability allowed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ReflectProbMin = 0f;

    /// <summary>
    /// If set, perform a simple multiplication on the reflect probability.
    /// <code>(reflectProb = chargeLevel * chargeProbScalar)</code>
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? ChargeProbScalar;
}
