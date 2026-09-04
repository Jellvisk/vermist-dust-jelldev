using System.Linq;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Components;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Content.Shared.NodeContainer;
using Content.Shared.NodeContainer.NodeGroups;
using Robust.Shared.Spawners;

namespace Content.Server._VDS.Barrier.Pulveris;

[NodeGroup(NodeGroupID.PulverisBarrier)]
public sealed class PulverisBarrierNodeGroup : BaseNodeGroup
{
    [Dependency]
    private readonly IEntityManager _entMan = default!;

    private EntityQuery<PulverisBarrierComponent> _barrierQuery;
    private EntityQuery<TimedDespawnComponent> _timedDespawnQuery;
    private EntityQuery<PulverisBarrierControllerComponent> _barrierControllerQuery;
    private EntityQuery<PulverisBarrierRelayComponent> _barrierRelayQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    /// <summary>
    /// Our master controller, if any.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Controllers = [];

    /// <summary>
    /// The barriers of this node group.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Barriers = [];

    /// <summary>
    /// The relays of this node group.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Relays = [];

    /// <summary>
    /// Invalid entities connected to the node group, soon to be removed.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Invalid = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public bool HasGrace = true;

    public override void Initialize(Node sourceNode, IEntityManager entMan)
    {
        base.Initialize(sourceNode, entMan);
        _barrierQuery = entMan.GetEntityQuery<PulverisBarrierComponent>();
        _timedDespawnQuery = entMan.GetEntityQuery<TimedDespawnComponent>();
        _barrierControllerQuery = entMan.GetEntityQuery<PulverisBarrierControllerComponent>();
        _barrierRelayQuery = entMan.GetEntityQuery<PulverisBarrierRelayComponent>();
        _xformQuery = entMan.GetEntityQuery<TransformComponent>();
    }

    public override void LoadNodes(List<Node> groupNodes)
    {
        base.LoadNodes(groupNodes);
        var pulverisBarrierSys = _entMan.System<SharedPulverisBarrierSystem>();
        var pulverisBarrierRelaySys = _entMan.System<SharedPulverisBarrierRelaySystem>();

        // add all nodes to cache. we'll further process them later.
        foreach (var node in groupNodes)
        {
            // if (AlreadyAdded(node.Owner))
            //     continue;

            if (_barrierRelayQuery.TryGetComponent(node.Owner, out var relayComp) && relayComp.Valid)
            {
                Relays.Add(node.Owner);
                pulverisBarrierRelaySys.ProcessRelay((node.Owner, relayComp));
            }

            if (_barrierQuery.TryGetComponent(node.Owner, out var barrierComp))
            {
                Barriers.Add(node.Owner);
            }
        }

        foreach (var node in groupNodes)
        {
            if (_barrierQuery.TryGetComponent(node.Owner, out var barrierComp))
            {
                // this is why we need to do two loops
                pulverisBarrierSys.TryUpdateBarrierRelayOwners((node.Owner, barrierComp), Relays);

                pulverisBarrierSys.ProcessBarrier((node.Owner, barrierComp));
            }

            if (_barrierRelayQuery.TryGetComponent(node.Owner, out var relayComponent))
            {
                pulverisBarrierRelaySys.ProcessRelay((node.Owner, relayComponent));
            }
        }
    }
}
