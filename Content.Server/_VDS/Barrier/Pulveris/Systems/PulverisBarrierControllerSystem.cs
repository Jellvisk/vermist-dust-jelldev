using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Components;
using Content.Shared._VDS.Barrier.Pulveris.Systems;

namespace Content.Server._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierControllerSystem : SharedPulverisBarrierControllerSystem
{
    [Dependency]
    private readonly SharedPulverisBarrierRelaySystem _barrierRelaySystem = default!;

    private EntityQuery<PulverisBarrierRelayComponent> _barrierRelayQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PulverisBarrierControllerComponent, PulverisBarrierControllerToggleButtonPressedEvent>(
            OnToggleButtonPressed
        );

        _barrierRelayQuery = GetEntityQuery<PulverisBarrierRelayComponent>();
    }

    private void OnToggleButtonPressed(
        Entity<PulverisBarrierControllerComponent> ent,
        ref PulverisBarrierControllerToggleButtonPressedEvent args)
    {
        TryToggleController(ent);
    }

    public bool TryToggleController(
        Entity<PulverisBarrierControllerComponent> controller,
        PulverisBarrierRelayComponent? relayComp = null)
    {
        if (!_barrierRelayQuery.Resolve(controller, ref relayComp))
            return false;

        if (!_barrierRelaySystem.TryGetLinkAhead((controller, relayComp), out var targetUid, out var targetComp))
        {
            return false;
        }

        _barrierRelaySystem.Link((controller, relayComp), (targetUid, targetComp));

        return true;
    }
}
