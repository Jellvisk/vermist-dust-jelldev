using Content.Shared._VDS.Barrier;
using Content.Shared._VDS.Barrier.Pulveris.Systems;

namespace Content.Server._VDS.Barrier.Pulveris.Systems;

public sealed class PulverisBarrierControllerSystem : SharedPulverisBarrierControllerSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PulverisBarrierControllerComponent, PulverisBarrierControllerToggleButtonPressedEvent>(OnToggleButtonPressed);
    }

    private void OnToggleButtonPressed(Entity<PulverisBarrierControllerComponent> ent, ref PulverisBarrierControllerToggleButtonPressedEvent args)
    {
        TryToggleController(ent);
    }

    public void TryToggleController(Entity<PulverisBarrierControllerComponent> ent)
    {
        Log.Debug($"You did it!!! {ToPrettyString(ent)}");
    }
}
