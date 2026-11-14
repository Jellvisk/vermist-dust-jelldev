using System.Numerics;
using Robust.Client.UserInterface;
using static Content.Client._VDS.UserInterface.Controls.VectorCanvas.VectorCanvas;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;

/// <summary>
/// Contract that states this is a <see cref="VectorCanvas"/> slot.
/// Positional slot that requests an <see cref="IVectorCanvasBrush"/> via <see cref="BrushKey"/> to fill and handle.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public partial interface IVectorCanvasSlot
{
    /// <summary>
    /// The brush this slot wants to be filled by.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    string BrushKey { get; set; }

    /// <summary>
    /// Add this slot to a canvas.
    /// </summary>
    /// <param name="canvas">The canvas to add to.</param>
    void AddSlot(VectorCanvas canvas);

    VectorCanvas Canvas { get; set; }

    /// <summary>
    /// Desired pixel position(s) of this slot.
    /// </summary>
    List<Vector2> SlotPixelPosition { get; }

    /// <summary>
    /// Desired pixel size(s) of this slot.
    /// </summary>
    List<Vector2> SlotPixelSize { get; }

    /// <summary>
    /// Desired position(s) of this slot on its <see cref="VectorCanvas"/>.
    /// </summary>
    List<Vector2> SlotPosition { get; }

    /// <summary>
    /// Desired slot scale(s).
    /// </summary>
    List<Vector2> SlotScale { get; }

    /// <summary>
    /// Minimum scale(s) of this slot.
    /// </summary>
    List<Vector2> SlotMinScale { get; set; }

    /// <summary>
    /// Maximum scale(s) of this slot.
    /// </summary>
    List<Vector2> SlotMaxScale { get; set; }

    /// <summary>
    ///
    /// </summary>
    List<Vector2> SlotElasticity { get; set; }

    SlotScaleType SlotScaleType { get; set; }
}

[Virtual]
public abstract partial class VectorCanvasSlot : IVectorCanvasSlot
{
    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public string BrushKey { get; set; } = "default";

    /// <inheritdoc/>
    public void AddSlot(VectorCanvas canvas)
    {
        canvas.Slots.Add(this);
    }

    private VectorCanvas _canvas;

    /// <inheritdoc/>
    public VectorCanvas Canvas
    {
        get => _canvas;
        set
        {
            _canvas = value;
            AddSlot(_canvas);
        }
    }

    // Position

    /// <inheritdoc/>
    public virtual List<Vector2> SlotPixelPosition => GetSlotPixelPosition();

    public abstract List<Vector2> GetSlotPixelPosition();

    /// <inheritdoc/>
    public virtual List<Vector2> SlotPixelSize => GetSlotPixelSize();

    public abstract List<Vector2> GetSlotPixelSize();

    /// <inheritdoc/>
    public virtual List<Vector2> SlotPosition => GetSlotPosition(Canvas);

    public virtual List<Vector2> GetSlotPosition(VectorCanvas canvas)
    {
        var pixelPos = GetSlotPixelPosition();

        var list = new List<Vector2>();

        foreach (var pos in pixelPos)
        {
            var fractionPos = canvas.PixelToLocal(pos);

            if (fractionPos.HasValue)
                list.Add(fractionPos.Value);
        }

        return list;
    }

    // Scale

    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual List<Vector2> SlotScale { get; set; } = [new Vector2(1f, 1f)];

    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual List<Vector2> SlotMinScale { get; set; } = [new Vector2(1f, 1f)];

    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual List<Vector2> SlotMaxScale { get; set; } = [new Vector2(1f, 1f)];

    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual List<Vector2> SlotElasticity { get; set; } = [new Vector2(0f, 0f)];

    /// <inheritdoc/>
    [ViewVariables(VVAccess.ReadWrite)]
    public SlotScaleType SlotScaleType { get; set; } = SlotScaleType.Fixed;
}
