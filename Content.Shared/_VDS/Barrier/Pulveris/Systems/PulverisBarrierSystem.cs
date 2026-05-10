using System.Linq;
using System.Numerics;
using Content.Shared._VDS.Physics;
using Content.Shared.NodeContainer;
using Content.Shared.Physics;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierSystem : EntitySystem
{
    [Dependency] private readonly ReflectiveRaycastSystem _raycastSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly RayCastSystem _rayCast = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiverSystem = default!;

    private EntityQuery<PulverisBarrierComponent> _holoQuery;
    private EntityQuery<PulverisBarrierControllerComponent> _holoControllerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _holoQuery = GetEntityQuery<PulverisBarrierComponent>();
        _holoControllerQuery = GetEntityQuery<PulverisBarrierControllerComponent>();
    }

    /// <summary>
    /// Iterates through our barriers and deletes invalid ones.
    /// </summary>
    public bool ValidateBarrier(Entity<PulverisBarrierComponent> barrier)
    {
        if (TerminatingOrDeleted(barrier))
            return false;

        if (IsValidBarrier(barrier))
            return true;

        TryQueueDel(barrier);

        return false;
    }

    public bool IsValidBarrier(Entity<PulverisBarrierComponent> barrier)
    {

        if (barrier.Comp.RequiresController && barrier.Comp.Controllers.Count == 0)
            return false;

        if (barrier.Comp.RequiresPower && !_powerReceiverSystem.IsPowered(barrier.Owner))
            return false;

        // if (barrier.Comp.RequiresController)
        //     return CheckBehindForController(barrier);

        return true;
    }


    // public bool CheckBehindForController(Entity<PulverisBarrierComponent> barrier, PulverisBarrierControllerComponent? controller = null)
    // {
    //     Log.Info("CHECKINGGGG");
    //     var holoXform = Transform(barrier);
    //     var (holoMapCoords, holoMapDir) = _transformSystem.GetWorldPositionRotation(holoXform);
    //
    //     // what we stop at (walls)
    //     var probeFilter = new QueryFilter
    //     {
    //         MaskBits = (int)CollisionGroup.FullTileMask,
    //         IsIgnored = ent => _holoControllerQuery.TryGetComponent(ent, out var controller) || _holoQuery.TryGetComponent(ent, out var _),
    //         Flags = QueryFlags.Static | QueryFlags.Dynamic
    //     };
    //
    //     // what we are looking for (a holo controller)
    //     var pathFilter = new QueryFilter
    //     {
    //         MaskBits = (int)CollisionGroup.AllMask,
    //         Flags = QueryFlags.Static | QueryFlags.Dynamic
    //     };
    //
    //     // define a new ray stat
    //     var ray = new ReflectiveRayState(
    //             probeFilter,
    //             pathFilter,
    //             origin: holoMapCoords,
    //             direction: holoMapDir.Opposite().ToWorldVec(),
    //             maxRange: 10f, // todo: range defined in controller based on power input
    //             holoXform.MapID
    //     );
    //     var (probeResult, pathResults) = _raycastSystem.CastAndUpdateReflectiveRayStateRef(ref ray);
    //     Log.Info($"Hnng {probeResult} and {pathResults.Hit}, {pathResults.Results.Count}");
    //     // var probe = _rayCast.CastRay(
    //     //         holoXform.MapID,
    //     //         holoMapCoords,
    //     //         holoMapDir.Opposite().ToWorldVec() * 10f,
    //     //         probeFilter);
    //     // var probeHit = probe.Results.FirstOrNull();
    //     //
    //     // var probeHitRange = (probeHit.HasValue)
    //     //     ? Vector2.Distance(_transformSystem.GetWorldPosition(probeHit.Value.Entity), holoMapCoords)
    //     //     : 10f;
    //     //
    //     // var path = _rayCast.CastRay(
    //     //         holoXform.MapID,
    //     //         holoMapCoords,
    //     //         holoMapDir.Opposite().ToWorldVec() * probeHitRange,
    //     //         pathFilter);
    //
    //     Log.Info($"{pathResults.Results.Any(ent => ent.Entity == barrier.Comp.Controller)}");
    //     return pathResults.Results.Any(ent => ent.Entity == barrier.Comp.Controller);
    // }

}
