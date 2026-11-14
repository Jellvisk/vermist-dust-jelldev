using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas.Brushes;

/// <summary>
/// Abstract canvas control to be used as a container or for events.
/// </summary>
public sealed class VectorCanvasControlMarker : Container
{
    [ViewVariables(VVAccess.ReadWrite)]
    public string Key { get; set; } = "control";

    public Vector2 InitialPixelPosition { get; set; }

    public Vector2? CanvasPosition { get; set; }

    public VectorCanvasControlMarker()
    {
        RobustXamlLoader.Load(this);
    }
}
