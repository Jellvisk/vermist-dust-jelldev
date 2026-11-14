using Robust.Client.Graphics;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;

/// <summary>
/// Simple circle brush, wrapper for <see cref="DrawingHandleScreen.DrawCircle(Vector2, float, Color, bool)"/>.
/// </summary>
[DataDefinition]
public sealed partial class BrushCircle : VectorCanvasBrush
{
    [ViewVariables(VVAccess.ReadWrite)]
    public override string Key { get; set; } = "circle";

    /// <summary>
    /// The radius of our circle.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Radius { get; set; }

    /// <summary>
    /// The color of our circle.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Color Color { get; set; }

    /// <summary>
    /// If our circle is filled or not.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Filled { get; set; }

    public override void DrawStroke(
        VectorCanvas canvas,
        IVectorCanvasSlot element,
        DrawingHandleScreen handle
    )
    {
        var pos = element.GetSlotPosition(canvas);

        // nothing to draw.
        if (pos.Count < 1)
            return;

        for (var count = 0; count < pos.Count; count++)
        {
            var point = canvas.LocalToPixel(pos[count], canvas.PixelSize);
            if (point.HasValue)
                handle.DrawCircle(point.Value, Radius, Color, Filled);
        }
    }
}

