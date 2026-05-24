using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._VDS.NodeContainer.Nodes;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Content.Shared.NodeContainer;
using Content.Shared.NodeContainer.NodeGroups;
using Robust.Shared.Utility;

namespace Content.Server._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierNodeSystem : SharedPulverisBarrierNodeSystem
{
    private EntityQuery<NodeContainerComponent> _nodeContainerQuery;

    public override void Initialize()
    {
        base.Initialize();
        _nodeContainerQuery = GetEntityQuery<NodeContainerComponent>();
    }

    public bool TryGetDirectionalNode(
        Entity<NodeContainerComponent?> ent,
        [NotNullWhen(true)] out DirectionalNode? directionalNode,
        string? name = null
    )
    {
        directionalNode = null;

        if (!_nodeContainerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        foreach (var node in ent.Comp.Nodes.Values)
        {
            if (node is DirectionalNode directional
                && (name == null || name == directional.Name))
            {
                directionalNode = directional;
                return true;
            }
        }

        return false;
    }

    public override bool TryGetPulverisBarrierNodeGroup(
        Entity<NodeContainerComponent?> ent,
        [NotNullWhen(true)] out INodeGroup? nodeGroup
    )
    {
        nodeGroup = null;

        if (!_nodeContainerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (
            !ent
                .Comp.Nodes.Values.Select(node => node.NodeGroup)
                .OfType<PulverisBarrierNodeGroup>()
                .TryFirstOrDefault(out var group)
        )
        {
            return false;
        }

        nodeGroup = group;
        return true;
    }
}
