using System.Diagnostics.CodeAnalysis;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.NodeContainer;
using Content.Shared.NodeContainer.NodeGroups;
using Robust.Shared.Map.Components;

namespace Content.Server._VDS.Barrier.Pulveris;

[NodeGroup(NodeGroupID.PulverisBarrier)]
public sealed class PulverisBarrierNodeGroup : BaseNodeGroup
{
    [Dependency]
    private readonly IEntityManager _entMan = default!;

    private EntityQuery<PulverisBarrierComponent> _barrierQuery;
    private EntityQuery<PulverisBarrierControllerComponent> _barrierControllerQuery;
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

    public override void Initialize(Node sourceNode, IEntityManager entMan)
    {
        base.Initialize(sourceNode, entMan);

        _barrierQuery = entMan.GetEntityQuery<PulverisBarrierComponent>();
        _barrierControllerQuery = entMan.GetEntityQuery<PulverisBarrierControllerComponent>();
        _xformQuery = entMan.GetEntityQuery<TransformComponent>();

    }

    public override void LoadNodes(List<Node> groupNodes)
    {
        base.LoadNodes(groupNodes);

        foreach (var node in groupNodes)
        {
            if (
                _barrierControllerQuery.TryComp(node.Owner, out var controller)
                && IsValidController((node.Owner, controller))
            )
            {
                Controllers.Add(node.Owner);
                continue;
            }

            Barriers.Add(node.Owner);
        }

        if (Barriers.Count > 0)
        {
            var pulverBarrierSys = _entMan.System<SharedPulverisBarrierSystem>();

            foreach (var barrier in Barriers)
            {
                if (!_barrierQuery.TryComp(barrier, out var barrierComp))
                    continue;

                barrierComp.Controllers = Controllers;

                if (pulverBarrierSys.TryUpdateBarrier((barrier, barrierComp)))
                    continue;

                Barriers.Remove(barrier);
            }

        }
        if (Controllers.Count > 0)
        {
            var pulverBarrierControllerSys = _entMan.System<SharedPulverisBarrierControllerSystem>();

            foreach (var controller in Controllers)
            {
                if (!_barrierControllerQuery.TryComp(controller, out var controllerComp))
                    continue;

                controllerComp.Connected = Barriers.Count > 0;
                pulverBarrierControllerSys.TryUpdateAppearance(controller);
            }

        }
    }
    private bool IsValidController([NotNullWhen(true)] Entity<PulverisBarrierControllerComponent>? controller)
    {
        if (!controller.HasValue)
            return false;

        if (!_xformQuery.TryGetComponent(controller.Value, out var xform))
            return false;

        if (!xform.GridUid.HasValue || !_entMan.TryGetComponent<MapGridComponent>(xform.GridUid.Value, out var grid) || grid == null)
            return false;

        return true;
    }
}
