using Content.Shared._VDS.Barrier;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._VDS.Barrier.Pulveris.UI;

[UsedImplicitly]
public sealed class PulverisBarrierControllerBoundUserInterface : BoundUserInterface
{
    private PulverisBarrierControllerWindow? _window;

    public PulverisBarrierControllerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<PulverisBarrierControllerWindow>();
        _window.OnToggleButtonPressed += () => SendMessage(new PulverisBarrierControllerToggleButtonPressedEvent());

    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);


    }

}
