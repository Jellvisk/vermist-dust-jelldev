// namespace Content.Shared._VDS.Barrier;
//
// public abstract partial class BarrierSystem : EntitySystem
// {
//
//     public override void Initialize()
//     {
//         base.Initialize();
//     }
//
//     public void AddToBarrier()
//     {
//         // spawn a single Barrier entity
//     }
//
//     public void CreateBarrier()
//     {
//         // create from x to y coords
//     }
//
//     public void CreateBarrier()
//     {
//         // create from x, y times, in z direction
//     }
//
//     public void DeleteBarrier()
//     {
//         // delete all
//     }
//
//     public void GetBarrier()
//     {
//         // return BarrierList
//     }
//
//     public void GetBarrierGroup()
//     {
//     }
//
//     public void RelayBarrierEvent<TEvent>()
//     {
//         // relay an event to its Barriered children and parents
//     }
//
//     public void SetJointType()
//     {
//     }
//
// }
//
// /// <summary>
// /// Raised when attempting to remove a Barrier entity.
// /// </summary>
// [ByRefEvent]
// public record struct AttemptRemoveFromBarrierEvent(Entity<BarrierComponent> Victim, bool Cancelled = false);
//
// /// <summary>
// /// Raised when attempting to add a Barrier entity.
// /// </summary>
// [ByRefEvent]
// public record struct AttemptAddToBarrierEvent(EntityUid Source, EntityCoordinates TargetCoords, bool Cancelled = false);
//
// /// <summary>
// /// Raised when a Barrier entity is created
// /// </summary>
// [ByRefEvent]
// public record struct AddToBarrierEvent(Entity<BarrierComponent> Barrier);
//
// /// <summary>
// /// Raised when a Barrier entity is removed.
// /// </summary>
// [ByRefEvent]
// public record struct RemoveFromBarrierEvent(Entity<BarrierComponent> Barrier);
//
// /// <summary>
// /// Raised to relay one event to all under this BarrierID
// /// </summary>
// [ByRefEvent]
// public readonly record struct BarrierRelayEvent();
