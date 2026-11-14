using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;

/// <summary>
/// Contract that states this is a brush that wishes to do something to <see cref="IVectorCanvasSlot"/>
/// </summary>
[ImplicitDataDefinitionForInheritors]
public partial interface IVectorCanvasBrush
{
    /// <summary>
    /// The brush key, or name. Can be any string. The brush will search for <see cref="IVectorCanvasSlot"/> requesting its key and draw for it.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    string Key { get; set; }

    /// <summary>
    /// Add this brush to the canvas.
    /// </summary>
    /// <param name="canvas">Canvas to add to</param>
    [ViewVariables(VVAccess.ReadWrite)]
    void AddBrush(VectorCanvas canvas);
}

/// <summary>
/// Contract that states this is a brush that wishes to draw ontop of <see cref="IVectorCanvasSlot"/>
/// </summary>
[ImplicitDataDefinitionForInheritors]
public partial interface IVectorCanvasPainter : IVectorCanvasBrush
{
    /// <summary>
    /// How this brush draws a single 'stroke', or provided vector(s).
    /// Do not manually call this unless you know what you're doing, add your brush to the canvas via <see cref="AddBrush(VectorCanvas)"/> instead.
    /// </summary>
    /// <param name="canvas">The canvas we are drawing on.</param>
    /// <param name="vector">The canvas data we have found and want to draw on</param>
    /// <param name="handle">Handle</param>
    void DrawStroke(VectorCanvas canvas, IVectorCanvasSlot vector, DrawingHandleScreen handle);

    /// <summary>
    /// How this brush draws multiple 'strokes', or provided vector(s).
    /// Do not manually call this unless you know what you're doing, add your brush to the canvas via <see cref="AddBrush(VectorCanvas)"/> instead.
    /// </summary>
    /// <param name="canvas">The canvas we are drawing on.</param>
    /// <param name="vectors">The list of canvas data we have found and want to draw on</param>
    /// <param name="handle">Handle</param>
    void DrawStrokes(VectorCanvas canvas, IEnumerable<IVectorCanvasSlot> vectors, DrawingHandleScreen handle);
}

[Virtual]
public abstract partial class VectorCanvasBrush : IVectorCanvasPainter, IEquatable<IVectorCanvasBrush>
{
    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public abstract string Key { get; set; }

    /// <inheritdoc/>
    public void AddBrush(VectorCanvas canvas)
    {
        canvas.Brushes.Add(this);
    }

    /// <inheritdoc/>
    public bool Equals(IVectorCanvasBrush? other)
    {
        return Key == other?.Key;
    }

    /// <inheritdoc/>
    public abstract void DrawStroke(VectorCanvas canvas, IVectorCanvasSlot element, DrawingHandleScreen handle);

    /// <inheritdoc/>
    public virtual void DrawStrokes(
        VectorCanvas canvas,
        IEnumerable<IVectorCanvasSlot> elements,
        DrawingHandleScreen handle
    )
    {
        foreach (var element in elements)
        {
            DrawStroke(canvas, element, handle);
        }
    }
}

[Virtual]
public abstract partial class VectorCanvasBrushControl : IVectorCanvasBrush, IEquatable<IVectorCanvasBrush>
{
    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public abstract string Key { get; set; }

    /// <inheritdoc/>
    public void AddBrush(VectorCanvas canvas)
    {
        canvas.Brushes.Add(this);
    }

    public abstract void AddControl(VectorCanvas canvas, IVectorCanvasSlot element);

    /// <inheritdoc/>
    public virtual void AddControls(
        VectorCanvas canvas,
        IEnumerable<IVectorCanvasSlot> elements
    )
    {
        foreach (var element in elements)
        {
            AddControl(canvas, element);
        }
    }

    /// <inheritdoc/>
    public bool Equals(IVectorCanvasBrush? other)
    {
        return Key == other?.Key;
    }
}
