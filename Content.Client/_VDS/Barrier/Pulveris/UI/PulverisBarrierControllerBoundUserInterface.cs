using System.Numerics;
using Content.Client._VDS.UserInterface.Controls;
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


        // var canvas = _window.FindControl<SimpleVectorCanvas>("Testy");
        // canvas.AddPoint("test1", new Vector2(50f,50f), 5f, Color.AliceBlue, Color.AntiqueWhite);
        // canvas.AddPoint("test2", new Vector2(100f,50f), 2f, Color.White, Color.Gray);
        // canvas.AddPoint("test3", new Vector2(88f,250f), 10f, Color.Green, Color.Gray);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
    }

}
