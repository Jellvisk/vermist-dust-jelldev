using System.Numerics;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;

/// <summary>
/// A straight line of vectors.
/// </summary>
[DataDefinition]
public sealed partial class VectorCanvasLine : VectorCanvasSlot
{
    private Vector2 _from;
    private Vector2 _to;

    private Vector2 _fromSize;
    private Vector2 _toSize;

    private int _steps;

    /// <summary>
    /// Our starting vector.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 From
    {
        get => _from;
        set => _from = value;
    }

    /// <summary>
    /// Vector to point towards.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 To
    {
        get => _to;
        set => _to = value;
    }

    /// <summary>
    /// Our starting vector size.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 FromSize
    {
        get => _fromSize;
        set => _fromSize = value;
    }

    /// <summary>
    /// Our ending vector size
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 ToSize
    {
        get => _toSize;
        set => _toSize = value;
    }

    /// <summary>
    /// How many extra vectors to draw between From and To.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int Steps
    {
        get => _steps;
        set => _steps = value;
    }

    private List<Vector2> GetStepped(Vector2 from, Vector2 to)
    {
        // not enough steps to divide from just return point A and B.
        if (Steps <= 2)
            return [from, to];

        var result = new List<Vector2>(Steps);
        for (var step = 0f; step < Steps; step++)
        {
            var percent = step / (Steps - 1);
            result[(int)step] = Vector2.Lerp(from, to, percent);
        }

        return result;
    }

    public override List<Vector2> GetSlotPixelPosition()
    {
        return GetStepped(From, To);
    }

    public override List<Vector2> GetSlotPixelSize()
    {
        return GetStepped(FromSize, ToSize);
    }
}
