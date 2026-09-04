using System.Linq;
using Content.Shared.NodeContainer;
using Content.Shared.Power;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._VDS.Barrier.Pulveris.Systems;

public abstract class SharedPulverisBarrierSystem : EntitySystem
{
    [Dependency]
    private readonly INetManager _net = default!;

    [Dependency]
    private readonly IGameTiming _timing = default!;

    private EntityQuery<NodeContainerComponent> _nodeContainerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _nodeContainerQuery = GetEntityQuery<NodeContainerComponent>();

        SubscribeLocalEvent<PulverisBarrierComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<PulverisBarrierComponent, AttemptPulverisBarrierDeactivateEvent>(
            OnAttemptPulverisBarrierDeactivate
        );
    }

    private void OnAttemptPulverisBarrierDeactivate(
        Entity<PulverisBarrierComponent> ent,
        ref AttemptPulverisBarrierDeactivateEvent args
    )
    {
        if (args.Cancelled)
            return;

        HandleInvalidBarrier(args.Barrier);
    }

    private void OnPowerChanged(Entity<PulverisBarrierComponent> ent, ref PowerChangedEvent args)
    {
        if (!ent.Comp.RequiresPower)
            return;

        if (!args.Powered)
        {
            var ev = new AttemptPulverisBarrierDeactivateEvent(ent);
            RaiseLocalEvent(ent, ref ev);
        }
    }

    public void ProcessBarrier(Entity<PulverisBarrierComponent> barrier)
    {
        if (BarrierHasValidConnections(barrier))
            return;

        Log.Debug("kill");

        HandleInvalidBarrier(barrier);
    }

    public void HandleInvalidBarrier(Entity<PulverisBarrierComponent> barrier)
    {
        TryQueueDel(barrier);
    }

    public bool TryUpdateBarrierRelayOwners(Entity<PulverisBarrierComponent> barrier, HashSet<EntityUid> relays)
    {
        if (relays.SequenceEqual(barrier.Comp.Relays))
            return false;

        barrier.Comp.Relays = relays;
        return true;
    }

    public bool BarrierHasRelay(Entity<PulverisBarrierComponent> barrier)
    {
        if (!barrier.Comp.RequiresRelay)
            return true;

        return barrier.Comp.Relays.Count != 0;
    }

    public virtual bool BarrierHasValidConnections(Entity<PulverisBarrierComponent> barrier)
    {
        // handled in server
        return true;
    }
}
