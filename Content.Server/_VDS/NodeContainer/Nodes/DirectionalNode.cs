using Content.Server.NodeContainer.Nodes;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;

namespace Content.Server._VDS.NodeContainer.Nodes;

/// <summary>
/// A <see cref="Node"/> that can reach other <see cref="DirectionalNode"/>s or <see cref="AdjacentNode"/> in its specified relative directions.
/// </summary>
[DataDefinition]
public sealed partial class DirectionalNode : Node, IRotatableNode
{

    /// <summary>
    /// What directions this node will accept as valid.
    /// </summary>
    [DataField]
    public DirectionFlag OpenDirections;

    /// <summary>
    /// What directions this node is currently connected to
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public DirectionFlag ConnectedDirections;

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
        var transformSys = entMan.System<SharedTransformSystem>();
        var pos = mapSys.TileIndicesFor((xform.GridUid.Value, grid), xform.Coordinates);
        var (worldPos, worldRot) = transformSys.GetWorldPositionRotation(xform);

        foreach (var worldDir in DirectionExtensions.AllDirections)
        {
            var targetIdx = pos.Offset(worldDir);

            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, targetIdx))
            {
                if (node.NodeGroupID != NodeGroupID)
                    continue;

                if (node is DirectionalNode directional)
                {
                    var target = node.Owner;

                    if (!xformQuery.TryGetComponent(target, out var targetXform))
                        continue;

                    var (targetWorldPos, targetWorldRot) = transformSys.GetWorldPositionRotation(targetXform);


                    var worldTargetDir = (worldPos - targetWorldPos).Normalized();
                    var worldSourceDir = (targetWorldPos - worldPos).Normalized();

                    // the direction of the target from the source
                    var localTargetDir = (-targetWorldRot).RotateVec(worldTargetDir).GetDir().GetOpposite();

                    // the direction of the source from the target
                    var localSourceDir = (-worldRot).RotateVec(worldSourceDir).GetDir().GetOpposite();

                    // Logger.Info($"-----");
                    // Logger.Info($"world source direction [from target]: {worldSourceDir.GetDir()}");
                    // Logger.Info($"world target direction [from source]: {worldTargetDir.GetDir()}");
                    //
                    // Logger.Info($"source direction [from target]: {localSourceDir}");
                    // Logger.Info($"target direction [from source]: {localTargetDir}");
                    //
                    // Logger.Info($"-----");
                    // entMan.SpawnAtPosition("EffectSparks", targetXform.Coordinates);

                    if (OpenDirections.HasFlag(localSourceDir.AsFlag())
                        && directional.OpenDirections.HasFlag(localTargetDir.AsFlag()))
                    {
                        directional.ConnectedDirections |= localSourceDir.AsFlag();

                        // valid!
                        yield return directional;
                    }
                    else
                    {
                        // unset flag
                        directional.ConnectedDirections &= ~localSourceDir.AsFlag();

                    }
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
        // TODO
        return true;
    }
}
