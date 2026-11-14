using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;

[DataDefinition]
public sealed partial class BrushStamp : VectorCanvasBrush
{
    [ViewVariables(VVAccess.ReadWrite)]
    public override string Key { get; set; } = "stamp";

    /// <summary>
    /// Texture path.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public string TexturePath { get; set; }

    /// <summary>
    /// Optional color to modulate the texture by.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Color? Color { get; set; }

    public override void DrawStroke(VectorCanvas canvas, IVectorCanvasSlot element, DrawingHandleScreen handle)
    {
        var pos = element.GetSlotPosition(canvas);
        var texture = canvas.GetTexture(TexturePath);

        if (pos.Count < 1)
            return;

        for (var count = 0; count < pos.Count; count++)
        {
            var point = canvas.LocalToPixel(pos[count], canvas.PixelSize);
            if (point.HasValue)
                handle.DrawTexture(texture, point.Value, Color);
        }
    }
}
