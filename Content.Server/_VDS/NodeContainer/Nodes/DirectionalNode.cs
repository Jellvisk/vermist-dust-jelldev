using Content.Server.NodeContainer.Nodes;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server._VDS.NodeContainer.Nodes;

/// <summary>
/// A <see cref="Node"/> that can reach other <see cref="DirectionalNode"/>s or <see cref="AdjacentNode"/> in its specified relative directions.
/// </summary>
[DataDefinition]
public sealed partial class DirectionalNode : Node, IRotatableNode
{
    /// <summary> What directions this node will accept as valid. </summary>
    [DataField]
    public DirectionFlag OpenDirections;

    /// <summary>
    /// What directions this node is currently connected to, and to what.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntityUid, DirectionFlag> ConnectedDirections = [];

    public override IEnumerable<Node> GetReachableNodes(
        TransformComponent xform,
        EntityQuery<NodeContainerComponent> nodeQuery,
        EntityQuery<TransformComponent> xformQuery,
        MapGridComponent? grid,
        IEntityManager entMan
    )
    {
        ConnectedDirections.Clear();

        if (!xform.Anchored || grid == null || xform.GridUid == null)
            yield break;

        var mapSys = entMan.System<SharedMapSystem>();
        var transformSys = entMan.System<SharedTransformSystem>();
        var pos = mapSys.TileIndicesFor((xform.GridUid.Value, grid), xform.Coordinates);
        var (worldPos, worldRot) = transformSys.GetWorldPositionRotation(xform);

        // we will keep track of what is connected to us in what direction for easy access.
        var dirs = new Dictionary<EntityUid, DirectionFlag>();

        foreach (var worldDir in DirectionExtensions.AllDirections)
        {
            var targetIdx = pos.Offset(worldDir);

            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, targetIdx))
            {
                if (node.NodeGroupID != NodeGroupID)
                    continue;

                if (node is not DirectionalNode and not AdjacentNode)
                    continue;

                // this is niche as fuck but it made me tear my hair out.
                // this fixes ghosts if you queue delete something to replace it with something else.
                if (entMan.IsQueuedForDeletion(node.Owner))
                    continue;

                var target = node.Owner;

                if (!xformQuery.TryGetComponent(target, out var targetXform))
                    continue;

                //  get our relative direction based on our own rotation
                var (targetWorldPos, targetWorldRot) = transformSys.GetWorldPositionRotation(targetXform);

                var worldTargetDir = (worldPos - targetWorldPos).Normalized();
                var worldSourceDir = (targetWorldPos - worldPos).Normalized();

                // the direction of the target from the source
                var localTargetDir = (-targetWorldRot).RotateVec(worldTargetDir).GetDir().GetOpposite();

                // the direction of the source from the target
                var localSourceDir = (-worldRot).RotateVec(worldSourceDir).GetDir().GetOpposite();

                if (
                    node is DirectionalNode directional
                    && OpenDirections.HasFlag(localSourceDir.AsFlag())
                    && directional.OpenDirections.HasFlag(localTargetDir.AsFlag())
                )
                {
                    if (dirs.TryAdd(target, localSourceDir.AsFlag()))
                    {
                        Logger.Debug($"ADDING {localSourceDir.AsFlag()}");
                        yield return directional;
                    }
                    else
                    {
                        DebugTools.Assert(
                            $"node {Name}, {entMan.ToPrettyString(Owner)} added {entMan.ToPrettyString(target)}, {localSourceDir} twice in a single iteration!"
                        );
                    }
                }
                else if (node is AdjacentNode adjacent && OpenDirections.HasFlag(localSourceDir.AsFlag()))
                {
                    if (dirs.TryAdd(target, localSourceDir.AsFlag()))
                    {
                        yield return adjacent;
                    }
                    else
                    {
                        DebugTools.Assert(
                            $"node {Name}, {entMan.ToPrettyString(Owner)} added {entMan.ToPrettyString(target)}, {localSourceDir} twice in a single iteration!"
                        );
                    }
                }
            }
        }

        ConnectedDirections = dirs;
    }

    public bool RotateNode(in MoveEvent ev)
    {
        return ev.NewRotation != ev.OldRotation;
    }
}
