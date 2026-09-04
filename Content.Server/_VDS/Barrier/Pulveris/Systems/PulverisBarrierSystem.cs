using System.Linq;
using Content.Server._VDS.NodeContainer.Nodes;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Content.Shared.NodeContainer;
using Robust.Shared.Spawners;
using Robust.Shared.Utility;

namespace Content.Server._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierSystem : SharedPulverisBarrierSystem
{
    [Dependency]
    private readonly NodeGroupSystem _nodeGroupSystem = default!;

    [Dependency]
    private readonly NodeContainerSystem _nodeContainerSystem = default!;

    private EntityQuery<NodeContainerComponent> _nodeContainerQuery;
    private EntityQuery<TimedDespawnComponent> _timedDespawnQuery;

    public override void Initialize()
    {
        base.Initialize();

        _nodeContainerQuery = GetEntityQuery<NodeContainerComponent>();
        _timedDespawnQuery = GetEntityQuery<TimedDespawnComponent>();
    }

    public override bool BarrierHasValidConnections(Entity<PulverisBarrierComponent> barrier)
    {
        if (!barrier.Comp.RequiresConnections)
            return false;

        var openDirs = DirectionFlag.None;
        var connectedDirs = DirectionFlag.None;
        var connectedCount = 0;

        if (!_nodeContainerQuery.TryComp(barrier, out var nodeContainer))
            return false;

        Log.Debug(ToPrettyString(barrier));

        if (
            !nodeContainer
                .Nodes.Values.Select(node => node)
                .OfType<DirectionalNode>()
                .TryFirstOrDefault(out var dirNode)
        )
        {
            Log.Debug("fuing null");
            return false;
        }

        if (dirNode.ConnectedDirections == null || dirNode.ConnectedDirections.Count == 0)
        {
            Log.Debug("fuck meeeee");
            return false;
        }

        foreach (var ent in dirNode.ConnectedDirections)
        {
            // do NOT count anything with a timedDespawn component, it's likely
            // a temporary startup effect that has yet to be cleared!!!
            if (_timedDespawnQuery.HasComp(ent.Key))
                continue;

            connectedDirs |= ent.Value;
            connectedCount++;
        }
        openDirs = dirNode.OpenDirections;

        // do our directions match?
        if ((connectedDirs & ~openDirs) != 0)
        {
            Log.Debug("contains bad direction");
            return false;
        }

        if (connectedCount < barrier.Comp.MinimumConnections || connectedCount > barrier.Comp.MaximumConnections)
        {
            Log.Debug($"too many or too little connections: {connectedCount}");
            return false;
        }

        return true;
    }

    // public override bool BarrierHasGrace(Entity<PulverisBarrierComponent> barrier, TimeSpan curTime)
    // {
    //     if (!_nodeContainerQuery.TryComp(barrier, out var nodeContainer))
    //         return false;
    //
    //     if (!nodeContainer
    //             .Nodes.Values.Select(node => node.NodeGroup)
    //             .OfType<PulverisBarrierNodeGroup>()
    //             .TryFirstOrDefault(out var group))
    //     {
    //         return false;
    //     }
    //
    //     if (!group.HasGrace)
    //         return false;
    //
    //     if (barrier.Comp.GracePeriod > curTime && barrier.Comp.HasGrace)
    //     {
    //         barrier.Comp.HasGrace = false;
    //         Dirty(barrier);
    //         Log.Debug($"we have grace cause we time grace time: {barrier.Comp.GracePeriod}      real time: {curTime}");
    //         Log.Debug($"we are now grace: {barrier.Comp.HasGrace}");
    //         var ev = new RefreshBarrierEvent(barrier);
    //         RaiseLocalEvent(barrier, ref ev);
    //         return true;
    //     }
    //
    //     return barrier.Comp.HasGrace;
    // }
}
