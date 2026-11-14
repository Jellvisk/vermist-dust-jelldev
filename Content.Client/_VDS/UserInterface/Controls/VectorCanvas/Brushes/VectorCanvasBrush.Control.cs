using System.Numerics;
using Content.Client._VDS.UserInterface.Controls.VectorCanvas.Brushes;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;

[DataDefinition]
public sealed partial class BrushControl : VectorCanvasBrushControl
{
    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public override string Key { get; set; } = "control";

    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Size { get; set; } = new(10f, 10f);

    public override void AddControl(VectorCanvas canvas, IVectorCanvasSlot element)
    {
        var pos = element.GetSlotPosition(canvas);

        if (pos.Count < 1)
            return;

        for (var count = 0; count < pos.Count; count++)
        {
            var marker = new VectorCanvasControlMarker
            {
                Key = Key,
                SetSize = Size,
                InitialPixelPosition = pos[count],
                CanvasPosition = canvas.PixelToLocal(pos[count])
            };
            canvas.AddChild(marker);
            // TODO: set position properly inside VectorCanvas.. how???

        }
    }
}
