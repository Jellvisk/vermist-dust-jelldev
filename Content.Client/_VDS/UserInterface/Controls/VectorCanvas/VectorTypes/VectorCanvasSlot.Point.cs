using System.Numerics;

namespace Content.Client._VDS.UserInterface.Controls.VectorCanvas;
/// <summary>
/// A single vector point.
/// </summary>
[DataDefinition]
public sealed partial class VectorCanvasPoint : VectorCanvasSlot
{
    private Vector2 _position;
    private Vector2 _size;

    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Size
    {
        get => _size;
        set => _size = value;
    }

    public override List<Vector2> GetSlotPixelPosition()
    {
        return [Position];
    }

    public override List<Vector2> GetSlotPixelSize()
    {
        return [Size];
    }
}
