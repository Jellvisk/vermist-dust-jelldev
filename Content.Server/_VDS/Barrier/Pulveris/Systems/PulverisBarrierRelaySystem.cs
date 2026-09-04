using System.Linq;
using Content.Server._VDS.NodeContainer.Nodes;
using Content.Shared._VDS.Barrier.Pulveris.Components;
using Content.Shared._VDS.Barrier.Pulveris.Systems;
using Content.Shared.NodeContainer;
using Robust.Shared.Spawners;

namespace Content.Server._VDS.Barrier.Pulveris.Systems;
public sealed partial class PulverisBarrierRelaySystem : SharedPulverisBarrierRelaySystem
{
    private EntityQuery<NodeContainerComponent> _nodeContainerQuery;
    private EntityQuery<TimedDespawnComponent> _timedDespawnQuery;

    [Dependency]
    private readonly SharedAppearanceSystem _appearance = default!;


    public override void Initialize()
    {
        base.Initialize();

        _nodeContainerQuery = GetEntityQuery<NodeContainerComponent>();
        _timedDespawnQuery = GetEntityQuery<TimedDespawnComponent>();
    }

    public override bool TryUpdateRelayConnection(Entity<PulverisBarrierRelayComponent?> relay, AppearanceComponent? appearance = null)
    {
        if (!Resolve(relay.Owner, ref relay.Comp))
            return false;

        if (!Resolve(relay, ref appearance))
            return false;

        if (!_nodeContainerQuery.TryComp(relay, out var nodeContainer))
            return false;

        var activeDirs = ConnectionVisualDir.None;
        var activatingDirs = ConnectionVisualDir.None;

        var node = nodeContainer
            .Nodes.Values.Select(node => node)
            .OfType<DirectionalNode>()
            .FirstOrDefault();

        if (node is not null)
        {
            foreach (var ent in node.ConnectedDirections)
            {
                if (_timedDespawnQuery.HasComp(ent.Key))
                {
                    activatingDirs |= ToVisualDirection(ent.Value);
                    Log.Debug($"setting activation cool state to {activatingDirs}");
                }
                else
                {
                    activeDirs |= ToVisualDirection(ent.Value);
                    Log.Debug($"setting visual state to {activeDirs}");
                }
            }
        }

        _appearance.SetData(
            relay.Owner,
            PulverisBarrierRelayVisuals.ConnectionVisualState,
            activeDirs,
            appearance
        );

        _appearance.SetData(
            relay.Owner,
            PulverisBarrierRelayVisuals.ConnectionVisualActivatingState,
            activatingDirs,
            appearance
        );

        // set connected = true if any directions are... connected...
        relay.Comp.Connected = activeDirs > 0;
        _appearance.QueueUpdate(relay, appearance);
        return true;
    }
}
