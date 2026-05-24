using System.Linq;
using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Chat;
using Content.Shared.NodeContainer;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
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
    public Dictionary<DirectionFlag, EntityUid> ConnectedDirections;

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

        // we will keep track of what is connected to us in what direction for easy access.
        var dirs = new Dictionary<DirectionFlag, EntityUid>();

        foreach (var worldDir in DirectionExtensions.AllDirections)
        {
            var targetIdx = pos.Offset(worldDir);

            foreach (var node in NodeHelpers.GetNodesInTile(nodeQuery, grid, targetIdx))
            {
                if (node.NodeGroupID != NodeGroupID)
                    continue;

                if (node is not DirectionalNode and not AdjacentNode)
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

                // Logger.Info($"-----");
                // Logger.Info($"world source direction [from target]: {worldSourceDir.GetDir()}");
                // Logger.Info($"world target direction [from source]: {worldTargetDir.GetDir()}");
                //
                // Logger.Info($"source direction [from target]: {localSourceDir}");
                // Logger.Info($"target direction [from source]: {localTargetDir}");
                //
                // Logger.Info($"-----");
                // entMan.SpawnAtPosition("EffectSparks", targetXform.Coordinates);

                if (
                    node is DirectionalNode directional
                    && OpenDirections.HasFlag(localSourceDir.AsFlag())
                    && directional.OpenDirections.HasFlag(localTargetDir.AsFlag())
                )
                {
                    dirs.Add(localSourceDir.AsFlag(), target);
                    yield return directional;
                }
                else if (node is AdjacentNode adjacent && OpenDirections.HasFlag(localSourceDir.AsFlag()))
                {
                    dirs.Add(localSourceDir.AsFlag(), target);
                    yield return adjacent;
                }
            }
        }

        // var pop = entMan.System<SharedPopupSystem>();
        // var msg1 = $"source ({entMan.ToPrettyString(xform.Owner)}) dirs - {ConnectedDirections}";
        // pop.PopupCoordinates(msg1, xform.Coordinates.Offset(new Vector2(0f, 0.5f)), PopupType.SmallCaution);
        // Logger.Debug(msg1);
        ConnectedDirections = dirs;
    }

    public bool RotateNode(in MoveEvent ev)
    {
        return ev.NewRotation != ev.OldRotation;
    }
}
