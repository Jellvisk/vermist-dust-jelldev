using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.NodeContainer.NodeGroups;
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
    [Dependency] private readonly IEntityManager _entMan = default!;

    private EntityQuery<HoloBarrierComponent> _holoQuery;
    private EntityQuery<HoloBarrierControllerComponent> _holoControllerQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize(Node sourceNode, IEntityManager entMan)
    {
        base.Initialize(sourceNode, entMan);

        _holoQuery = entMan.GetEntityQuery<HoloBarrierComponent>();
        _holoControllerQuery = entMan.GetEntityQuery<HoloBarrierControllerComponent>();
        _xformQuery = entMan.GetEntityQuery<TransformComponent>();
    }

    /// <summary>
    /// The entities currently belonging to this hologram node group.
    /// </summary>
    [ViewVariables]
    public HashSet<Entity<HoloBarrierComponent>> Members = [];

    /// <summary>
    /// Our master controller, if any.
    /// </summary>
    [ViewVariables]
    public Entity<HoloBarrierControllerComponent>? Controller;

    public override void LoadNodes(List<Node> groupNodes)
    {
        base.LoadNodes(groupNodes);

        Entity<HoloBarrierControllerComponent>? controller = null;
        foreach (var node in groupNodes)
        {
            var nodeOwner = node.Owner;

            // check if this node is the controller first
            if (_holoControllerQuery.TryGetComponent(nodeOwner, out var holoController))
            {
                Logger.Info($"added a potential controller guy {holoController}");
                controller = (nodeOwner, holoController);
                continue;
            }

            if (_holoQuery.TryGetComponent(nodeOwner, out var holo))
            {
                Logger.Info($"added a holo guy {holo}");
                Members.Add((nodeOwner, holo));
                continue;
            }

        }

        var holoBarrierSys = _entMan.System<HoloBarrierSystem>();
        if (IsValidController(controller))
        {
            Controller = controller.Value;
            Controller.Value.Comp.Members = Members;
            holoBarrierSys.UpdateHoloBarriers(Members, Controller.Value);
        }
        else
        {
            Controller = null;
            holoBarrierSys.UpdateHoloBarriers(Members);
            Members.Clear();
        }

    }

    private bool IsValidController([NotNullWhen(true)] Entity<HoloBarrierControllerComponent>? controller)
    {
        if (!controller.HasValue)
            return false;

        if (!_xformQuery.TryGetComponent(controller.Value, out var xform))
            return false;

        if (!xform.GridUid.HasValue
            || !_entMan.TryGetComponent<MapGridComponent>(xform.GridUid.Value, out var grid))
            return false;

        return true;
    }
}
