using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._VDS.Physics;

public sealed partial class ChainSystem : EntitySystem
{
    [Dependency]
    private readonly SharedJointSystem _joint = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void AddToChain()
    {
        // spawn a single Chain entity
    }

    public void CreateChain(ChainMask mask)
    {
        // create from x to y coords
    }

    public void CreateChain()
    {
        // create from x, y times, in z direction
    }

    public void DeleteChain()
    {
        // delete all
    }

    public void GetChain()
    {
        // return ChainList
    }

    public void GetChainGroup()
    {
    }

    public void RelayChainEvent<TEvent>()
    {
        // relay an event to its Chained children and parents
    }

    public void SetJointType()
    {
    }
}

/// <summary>
/// Raised when attempting to remove a Chain entity.
/// </summary>
[ByRefEvent]
public record struct AttemptRemoveFromChainEvent(Entity<ChainComponent> Victim, bool Cancelled = false);

/// <summary>
/// Raised when attempting to add a Chain entity.
/// </summary>
[ByRefEvent]
public record struct AttemptAddToChainEvent(EntityUid Source, EntityCoordinates TargetCoords, bool Cancelled = false);

/// <summary>
/// Raised when a Chain entity is created
/// </summary>
[ByRefEvent]
public record struct AddToChainEvent(Entity<ChainComponent> Chain);

/// <summary>
/// Raised when a Chain entity is removed.
/// </summary>
[ByRefEvent]
public record struct RemoveFromChainEvent(Entity<ChainComponent> Chain);

/// <summary>
/// Raised to relay one event to all under this ChainID
/// </summary>
[ByRefEvent]
public readonly record struct ChainRelayEvent();

public enum ChainMask
{
    Single,
    Line,
    Box
}
