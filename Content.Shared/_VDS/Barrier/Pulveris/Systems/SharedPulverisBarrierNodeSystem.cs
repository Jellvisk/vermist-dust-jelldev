using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Content.Shared.NodeContainer;
using Content.Shared.NodeContainer.NodeGroups;

namespace Content.Shared._VDS.Barrier.Pulveris.Systems;

public abstract class SharedPulverisBarrierNodeSystem : EntitySystem
{
    private EntityQuery<NodeContainerComponent> _nodeContainerQuery;
    public override void Initialize()
    {
        base.Initialize();

        _nodeContainerQuery = GetEntityQuery<NodeContainerComponent>();
    }

    public virtual bool TryGetDirectionalNode(
        Entity<NodeContainerComponent?> ent,
        [NotNullWhen(true)] out Node? node

    )
    {
        node = null;
        return false;
    }
    public static IEnumerable<Node> GetNode(
        Entity<NodeContainerComponent> ent
    )
    {
        foreach (var node in ent.Comp.Nodes.Values)
        {
            yield return node;
        }
    }

    public virtual bool TryGetPulverisBarrierNodeGroup(
            Entity<NodeContainerComponent?> ent,
            [NotNullWhen(true)] out INodeGroup? nodeGroup
            )
    {
        nodeGroup = null;
        return false;
    }



}
