using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Shared._VDS.Barrier.Pulveris.Components;
using Content.Shared._VDS.Physics;
using Content.Shared.NodeContainer;
using Content.Shared.Physics;
using Content.Shared.Power;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Spawners;
using Robust.Shared.Utility;

namespace Content.Shared._VDS.Barrier.Pulveris.Systems;

public abstract class SharedPulverisBarrierRelaySystem : EntitySystem
{
    [Dependency]
    private readonly SharedAppearanceSystem _appearance = default!;

    [Dependency]
    private readonly ReflectiveRaycastSystem _raycastSystem = default!;

    [Dependency]
    private readonly SharedTransformSystem _transformSystem = default!;

    [Dependency]
    private readonly SharedMapSystem _mapSystem = default!;

    [Dependency]
    private readonly SharedNodeContainerSystem _nodeContainerSystem = default!;

    [Dependency]
    private readonly IMapManager _mapManager = default!;

    [Dependency]
    private readonly EntityLookupSystem _lookupSystem = default!;

    private EntityQuery<PulverisBarrierComponent> _barrierQuery;
    private EntityQuery<PulverisBarrierRelayComponent> _barrierRelayQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PulverisBarrierRelayComponent, PowerChangedEvent>(OnPowerChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _barrierQuery = GetEntityQuery<PulverisBarrierComponent>();
        _barrierRelayQuery = GetEntityQuery<PulverisBarrierRelayComponent>();
    }

    private void OnPowerChanged(Entity<PulverisBarrierRelayComponent> ent, ref PowerChangedEvent args)
    {
        // to avoid dirty spam, we check if the state is different from our current valid state or not.
        // if (args.Powered == ent.Comp.Valid)
        //     return;
        //
        // ent.Comp.Valid = args.Powered;
        // Dirty(ent);
    }

    public void ProcessRelay(Entity<PulverisBarrierRelayComponent> relay)
    {
        TryUpdateRelayConnection(relay.AsNullable());
    }

    public bool TryGetLinkAhead(
        Entity<PulverisBarrierRelayComponent> relay,
        [NotNullWhen(true)] out EntityUid otherUid,
        [NotNullWhen(true)] out PulverisBarrierRelayComponent? otherComp
    )
    {
        otherUid = EntityUid.Invalid;
        otherComp = null;

        // early return if source isn't flagged as valid.
        if (!relay.Comp.Valid)
            return false;

        var relayXForm = Transform(relay);
        var (relayMapCoords, relayMapDir) = _transformSystem.GetWorldPositionRotation(relayXForm);

        // what we stop at (walls)
        var probeFilter = new QueryFilter
        {
            MaskBits = (int)CollisionGroup.FullTileMask,
            IsIgnored = ent => _barrierRelayQuery.TryComp(ent, out var _) || _barrierQuery.TryComp(ent, out var _),
            Flags = QueryFlags.Static | QueryFlags.Dynamic,
        };

        // what we are looking for (a holo controller)
        var pathFilter = new QueryFilter
        {
            MaskBits = (int)CollisionGroup.AllMask,
            IsIgnored = ent => !_barrierRelayQuery.TryComp(ent, out var _),
            Flags = QueryFlags.Static | QueryFlags.Dynamic,
        };

        // define a new ray state
        var ray = new ReflectiveRayState(
            probeFilter,
            pathFilter,
            origin: relayMapCoords,
            direction: relayMapDir.ToWorldVec(),
            maxRange: 10f, // todo: range defined in controller based on power input
            relayXForm.MapID
        );

        // update our ray by reference
        var (probeResult, pathResults) = _raycastSystem.CastAndUpdateReflectiveRayStateRef(ref ray);

        Log.Debug(
            $"Our path has {pathResults.Results.Count} results, {ToPrettyString(pathResults.Results.FirstOrDefault().Entity)} is the first."
        );

        otherUid = pathResults.Results.FirstOrDefault().Entity;

        if (!otherUid.IsValid() && !TerminatingOrDeleted(otherUid))
            return false;

        // make sure the thing we hit is valid as well.
        if (!_barrierRelayQuery.TryComp(otherUid, out var comp) || !comp.Valid)
            return false;

        otherComp = comp;

        return true;
    }

    public void Link(Entity<PulverisBarrierRelayComponent> source, Entity<PulverisBarrierRelayComponent> target)
    {
        var sourceXForm = Transform(source);
        var targetXForm = Transform(target);

        var (sourceWorldPos, sourceWorldRot) = _transformSystem.GetWorldPositionRotation(sourceXForm);

        var worldDirection = _transformSystem.GetWorldPosition(targetXForm) - sourceWorldPos;
        var parentRot = sourceWorldRot - sourceXForm.LocalRotation;

        var localDirection = (-parentRot).RotateVec(worldDirection).Rounded();
        Log.Debug($"worlddir: {worldDirection}");
        Log.Debug($"localdir: {localDirection}");

        if (localDirection.LengthSquared() < 0.1f)
        {
            Log.Warning(
                $"Unable to link {ToPrettyString(source)} and {ToPrettyString(target)}, localDirection {localDirection} is is invalid."
            );
            return;
        }

        // x or y becomes our total step count, whichever is higher. make sure they're positive since coordinates can be
        // negative.
        var totalSteps = Math.Max(Math.Abs(localDirection.X), Math.Abs(localDirection.Y));

        var step_vector_x = localDirection.X / totalSteps;
        var step_vector_y = localDirection.Y / totalSteps;

        if (totalSteps <= 0)
        {
            Log.Warning(
                $"Unable to link {ToPrettyString(source)} and {ToPrettyString(target)}, totalSteps is {totalSteps}."
            );
            return;
        }

        for (var step = 1; step < totalSteps; step++)
        {
            var current_x = step * step_vector_x;
            var current_y = step * step_vector_y;
            var vec = new Vector2(current_x, current_y);
            var newVec = (-sourceXForm.LocalRotation).RotateVec(vec);
            var spawnCoords = new EntityCoordinates(source, newVec);

            Log.Debug($"raw : {vec}");
            Log.Debug($"Spawning at: {spawnCoords}");
            PredictedSpawnAttachedTo("EffectPulverisBarrierActivate", spawnCoords);
        }

        TryUpdateRelayConnection(source.AsNullable());
        TryUpdateRelayConnection(target.AsNullable());
    }

    public virtual bool TryUpdateRelayConnection(Entity<PulverisBarrierRelayComponent?> relay, AppearanceComponent? appearance = null)
    {
        if (!Resolve(relay.Owner, ref relay.Comp))
            return false;

        if (!Resolve(relay, ref appearance))
            return false;

        // handled in server

        return relay.Comp.Connected;
    }

    protected virtual void PlayAnimation(EntityUid uid, string stateId, string animationKey)
    {

    }

    public static ConnectionVisualDir ToVisualDirection(DirectionFlag dir)
    {
        var visualDir = ConnectionVisualDir.None;

        var map = new[]
        {
            (DirectionFlag.South, ConnectionVisualDir.South),
            (DirectionFlag.East, ConnectionVisualDir.East),
            (DirectionFlag.North, ConnectionVisualDir.North),
            (DirectionFlag.West, ConnectionVisualDir.West),
        };

        foreach (var (from, to) in map)
        {
            if (dir.HasFlag(from))
                visualDir |= to;
        }

        return visualDir;
    }
}
