using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Holo.Systems;
using Content.Shared.NodeContainer;
using Content.Shared.NodeContainer.NodeGroups;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Server._VDS.Barrier.Holo;

[NodeGroup(NodeGroupID.HoloBarrier)]
public sealed partial class HoloBarrierNodeGroup : BaseNodeGroup
{
    [Dependency]
    private readonly IEntityManager _entMan = default!;

    private EntityQuery<HoloBarrierComponent> _holoQuery;
    private EntityQuery<HoloBarrierControllerComponent> _holoControllerQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    /// <summary>
    /// Our master controller, if any.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<HoloBarrierControllerComponent> Controller;

    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Members = [];

    public override void Initialize(Node sourceNode, IEntityManager entMan)
    {
        base.Initialize(sourceNode, entMan);

        _holoQuery = entMan.GetEntityQuery<HoloBarrierComponent>();
        _holoControllerQuery = entMan.GetEntityQuery<HoloBarrierControllerComponent>();
        _xformQuery = entMan.GetEntityQuery<TransformComponent>();
    }

    public override void LoadNodes(List<Node> groupNodes)
    {
        base.LoadNodes(groupNodes);

        foreach (var node in groupNodes)
        {
            if (_holoControllerQuery.TryComp(node.Owner, out var controller)
                && IsValidController((node.Owner, controller)))
            {
                Controller = (node.Owner, controller);
                continue;
            }

            Members.Add(node.Owner);
        }

        if (Members.Count > 0)
        {
            var holoBarrierSys = _entMan.System<HoloBarrierSystem>();
            foreach (var member in Members)
            {
                if (!_holoQuery.TryComp(member, out var holo))
                    continue;

                holo.Controller = Controller;
                if (holoBarrierSys.IsValidHoloBarrier((member, holo)))
                    continue;

                Members.Remove(member);
                _entMan.PredictedQueueDeleteEntity(member);
            }
        }
    }

    private bool IsValidController([NotNullWhen(true)] Entity<HoloBarrierControllerComponent>? controller)
    {
        if (!controller.HasValue)
            return false;

        if (!_xformQuery.TryGetComponent(controller.Value, out var xform))
            return false;

        if (!xform.GridUid.HasValue || !_entMan.TryGetComponent<MapGridComponent>(xform.GridUid.Value, out var grid))
            return false;

        return true;
    }
}

/// <summary>
/// A <see cref="Node"/> that can reach other <see cref="DirectionalNode"/>s or <see cref="NodeContainer.Nodes.AdjacentNode"/> in the specified rotations.
/// </summary>
[DataDefinition]
public sealed partial class DirectionalNode : Node, IRotatableNode
{
    [DataField]
    public DirectionFlag OpenDirections;

    [DataField]
    public bool RelativeToEntity = true;

    // public override IEnumerable<Node> GetReachableNodes(
    //     TransformComponent xform,
    //     EntityQuery<NodeContainerComponent> nodeQuery,
    //     EntityQuery<TransformComponent> xformQuery,
    //     MapGridComponent? grid,
    //     IEntityManager entMan)
    // {
    //     if (!xform.Anchored || grid == null || xform.GridUid == null)
    //         yield break;
    //
    //
    //     var mapSys = entMan.System<SharedMapSystem>();
    //
    //     var pos = mapSys.TileIndicesFor((xform.GridUid.Value, grid), xform.Coordinates);
    //     var localDir = xform.LocalRotation;
    //
    //     foreach (var dir in DirectionExtensions.AllDirections)
    //     {
    //         if (!dir.AsFlag().HasFlag(OpenDirections))
    //             continue;
    //
    //         var targetDir = localDir.RotateVec(dir.ToVec()).GetDir();
    //
    //         var targetIdx = pos.Offset(targetDir);
    //
    //         foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, targetIdx))
    //         {
    //             if (node.NodeGroupID != NodeGroupID)
    //                 continue;
    //
    //             var entity = node.Owner;
    //             var entityXform = xformQuery.GetComponent(entity);
    //             var entityDir = entityXform.LocalRotation;
    //
    //             if (node is DirectionalNode directional)
    //             {
    //                 foreach (var otherDir in DirectionExtensions.AllDirections)
    //                 {
    //                     if (!otherDir.AsFlag().HasFlag(directional.OpenDirections))
    //                         continue;
    //
    //                     var entTargetDir = entityDir.RotateVec(otherDir.ToVec()).GetDir();
    //                     if (entTargetDir == dir)
    //                         yield return directional;
    //                 }
    //             }
    //
    //             if (node is AdjacentNode adjacent)
    //                 yield return adjacent
    //
    //         }
    //     }
    // }

    public override IEnumerable<Node> GetReachableNodes(
        TransformComponent xform,
        EntityQuery<NodeContainerComponent> nodeQuery,
        EntityQuery<TransformComponent> xformQuery,
        MapGridComponent? grid,
        IEntityManager entMan
    )
    {
        if (!xform.Anchored || grid == null || xform.GridUid == null)
            yield break;

        var mapSys = entMan.System<SharedMapSystem>();
        var pos = mapSys.TileIndicesFor((xform.GridUid.Value, grid), xform.Coordinates);
        var sourceRot = xform.LocalRotation;

        foreach (var localDir in DirectionExtensions.AllDirections)
        {
            if ((localDir.AsFlag() & OpenDirections) == 0)
                continue;

            // Calculate world direction from source to target
            var targetWorldDir = sourceRot.RotateVec(localDir.ToVec()).GetDir();
            var targetIdx = pos.Offset(targetWorldDir);

            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, targetIdx))
            {
                if (node.NodeGroupID != NodeGroupID)
                    continue;

                var entity = node.Owner;
                var entityXform = xformQuery.GetComponent(entity);
                var targetRot = entityXform.LocalRotation;

                if (node is DirectionalNode directional)
                {
                    var requiredWorldDir = targetWorldDir.GetOpposite();

                    // Rotate the required world direction into the target's local space
                    // to check if it actually opens in that direction.
                    var localReqVec = (-targetRot).RotateVec(requiredWorldDir.ToVec());
                    var localReqDir = localReqVec.GetDir();

                    if ((localReqDir.AsFlag() & directional.OpenDirections) != 0)
                        yield return directional;
                }
                else if (node is AdjacentNode adjacent)
                {
                    yield return adjacent;
                }
            }
        }
    }

    public bool RotateNode(in MoveEvent ev)
    {
        if (!RelativeToEntity)
            return false;

        return true;
    }
}
