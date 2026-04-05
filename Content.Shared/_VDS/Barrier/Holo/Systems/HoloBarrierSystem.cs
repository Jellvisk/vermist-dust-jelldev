using System.Linq;
using System.Numerics;
using Content.Shared._VDS.Physics;
using Content.Shared.NodeContainer;
using Content.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._VDS.Barrier.Holo.Systems;

public sealed partial class HoloBarrierSystem : EntitySystem
{
    [Dependency] private readonly ReflectiveRaycastSystem _raycastSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly RayCastSystem _rayCast = default!;

    private EntityQuery<HoloBarrierComponent> _holoQuery;
    private EntityQuery<HoloBarrierControllerComponent> _holoControllerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _holoQuery = GetEntityQuery<HoloBarrierComponent>();
        _holoControllerQuery = GetEntityQuery<HoloBarrierControllerComponent>();
    }

    /// <summary>
    /// Iterates through our holobarriers and deletes invalid ones.
    /// </summary>
    public bool ValidateHoloBarrier(Entity<HoloBarrierComponent> barrier)
    {
        if (IsValidHoloBarrier(barrier))
            return true;

        PredictedQueueDel(barrier);
        return false;
    }

    public bool IsValidHoloBarrier(Entity<HoloBarrierComponent> barrier)
    {
        if (TerminatingOrDeleted(barrier))
            return false;

        if (barrier.Comp.Controller.IsValid() || TerminatingOrDeleted(barrier.Comp.Controller))
            return false;

        if (barrier.Comp.RequiresController)
            return CheckBehindForController(barrier);

        return true;
    }

    public bool CheckBehindForController(Entity<HoloBarrierComponent> barrier, HoloBarrierControllerComponent? controller = null)
    {
        Log.Info("CHECKINGGGG");
        var holoXform = Transform(barrier);
        var (holoMapCoords, holoMapDir) = _transformSystem.GetWorldPositionRotation(holoXform);

        // what we stop at (walls)
        var probeFilter = new QueryFilter
        {
            MaskBits = (int)CollisionGroup.FullTileMask,
            IsIgnored = ent => _holoControllerQuery.TryGetComponent(ent, out var controller) || _holoQuery.TryGetComponent(ent, out var _),
            Flags = QueryFlags.Static | QueryFlags.Dynamic
        };

        // what we are looking for (a holo controller)
        var pathFilter = new QueryFilter
        {
            MaskBits = (int)CollisionGroup.AllMask,
            Flags = QueryFlags.Static | QueryFlags.Dynamic
        };

        // define a new ray stat
        var ray = new ReflectiveRayState(
                probeFilter,
                pathFilter,
                origin: holoMapCoords,
                direction: holoMapDir.Opposite().ToWorldVec(),
                maxRange: 10f, // todo: range defined in controller based on power input
                holoXform.MapID
        );
        var (probeResult, pathResults) = _raycastSystem.CastAndUpdateReflectiveRayStateRef(ref ray);
        Log.Info($"Hnng {probeResult} and {pathResults.Hit}, {pathResults.Results.Count}");
        // var probe = _rayCast.CastRay(
        //         holoXform.MapID,
        //         holoMapCoords,
        //         holoMapDir.Opposite().ToWorldVec() * 10f,
        //         probeFilter);
        // var probeHit = probe.Results.FirstOrNull();
        //
        // var probeHitRange = (probeHit.HasValue)
        //     ? Vector2.Distance(_transformSystem.GetWorldPosition(probeHit.Value.Entity), holoMapCoords)
        //     : 10f;
        //
        // var path = _rayCast.CastRay(
        //         holoXform.MapID,
        //         holoMapCoords,
        //         holoMapDir.Opposite().ToWorldVec() * probeHitRange,
        //         pathFilter);

        Log.Info($"{pathResults.Results.Any(ent => ent.Entity == barrier.Comp.Controller)}");
        return pathResults.Results.Any(ent => ent.Entity == barrier.Comp.Controller);
    }

}
