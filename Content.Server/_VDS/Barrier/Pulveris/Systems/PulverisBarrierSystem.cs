using Content.Server._VDS.NodeContainer.Nodes;
using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Content.Shared.NodeContainer;

namespace Content.Server._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierSystem : SharedPulverisBarrierSystem
{
    [Dependency] private readonly PulverisBarrierNodeSystem _barrierNodeSystem = default!;

    public override bool TryValidateBarrier(Entity<PulverisBarrierComponent> barrier, NodeContainerComponent? nodeContainer = null)
    {
        if (!base.TryValidateBarrier(barrier, nodeContainer))
            return false;

        if (barrier.Comp.RequiresConnections
            && _barrierNodeSystem.TryGetDirectionalNode(barrier.Owner, out var node))
        {
            var dirs = DirectionFlag.None;
            foreach (var connectedDirs in node.ConnectedDirections.Keys)
            {
                dirs |= connectedDirs;
            }

            // do our directions match?
            if (dirs.HasFlag(node.OpenDirections))
                return true;
        }

        return false;
    }

}
