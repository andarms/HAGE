namespace Hmz.Core.Graphics._3D;

using System.Numerics;

public class Cube
{
  public const uint VertexCount = 8;
  public const uint IndexCount = 36;

  public float Size { get; set; } = 1f;
  public Transform Transform { get; set; } = new Transform();

  public Vector3[] GetVertices()
  {
    float halfSize = Size / 2f;
    return
    [
      new(-halfSize, -halfSize, -halfSize),
      new(halfSize, -halfSize, -halfSize),
      new(halfSize, halfSize, -halfSize),
      new(-halfSize, halfSize, -halfSize),
      new(-halfSize, -halfSize, halfSize),
      new(halfSize, -halfSize, halfSize),
      new(halfSize, halfSize, halfSize),
      new(-halfSize, halfSize, halfSize)
    ];
  }

  public int[] GetIndices()
  {
    return
    [
      0, 1, 2, 2, 3, 0,
      4, 5, 6, 6, 7, 4,
      0, 1, 5, 5, 4, 0,
      2, 3, 7, 7, 6, 2,
      0, 3, 7, 7, 4, 0,
      1, 2, 6, 6, 5, 1
    ];
  }

  public Matrix4x4 GetModelMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y, Transform.Rotation.X, Transform.Rotation.Z);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    var sizeScale = Matrix4x4.CreateScale(Size);
    return sizeScale * scale * rotation * translation;
  }
}
