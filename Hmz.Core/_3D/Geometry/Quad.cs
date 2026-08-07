namespace Hmz.Core._3D.Geometry;

using System.Numerics;
using Hmz.Core.Spatial;

// A flat, unit quad on the XZ plane (facing +Y up), centered at the origin - the ground-decal
// primitive behind things like blob shadows. Unlike Cube/Sphere it carries UVs, since decals
// are always textured.
public class Quad
{
  public const uint VertexCount = 4;
  public const uint IndexCount = 6;

  public Vector2 Size { get; set; } = Vector2.One;
  public Transform Transform { get; set; } = new Transform();

  public Quad() { }

  // Interleaved position(3) + texcoord(2) per vertex, matching the layout every other 3D
  // draw call in this file uses.
  public float[] GetVertices()
  {
    Vector2 half = Size / 2f;
    return
    [
      -half.X, 0f, -half.Y, 0f, 0f,
       half.X, 0f, -half.Y, 1f, 0f,
       half.X, 0f, half.Y, 1f, 1f,
      -half.X, 0f, half.Y, 0f, 1f,
    ];
  }

  public int[] GetIndices() => [0, 1, 2, 2, 3, 0];

  public Matrix4x4 GetRenderMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Transform.Rotation);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    Matrix4x4 sizeScale = Matrix4x4.CreateScale(Size.X, 1f, Size.Y);
    return sizeScale * scale * rotation * translation;
  }
}
