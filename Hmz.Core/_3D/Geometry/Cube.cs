namespace Hmz.Core._3D.Geometry;

using System.Numerics;
using Hmz.Core.Spatial;

public class Cube
{
  public const uint VertexCount = 8;
  public const uint IndexCount = 36;

  public Vector3 Size { get; set; } = Vector3.One;
  public Transform Transform { get; set; } = new Transform();

  public Cube() { }

  public Cube(Vector3 minCorner, float width, float height, float depth)
  {
    Size = new Vector3(width, height, depth);
    Transform.Position = minCorner + Size / 2f;
  }

  public Vector3[] GetVertices()
  {
    Vector3 half = Size / 2f;
    return
    [
      new(-half.X, -half.Y, -half.Z),
      new(half.X, -half.Y, -half.Z),
      new(half.X, half.Y, -half.Z),
      new(-half.X, half.Y, -half.Z),
      new(-half.X, -half.Y, half.Z),
      new(half.X, -half.Y, half.Z),
      new(half.X, half.Y, half.Z),
      new(-half.X, half.Y, half.Z)
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

  public Matrix4x4 GetRenderMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Transform.Rotation);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    var sizeScale = Matrix4x4.CreateScale(Size);
    return sizeScale * scale * rotation * translation;
  }
}
