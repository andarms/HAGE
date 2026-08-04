namespace Hmz.Core._3D.Geometry;

using System.Numerics;
using Hmz.Core.Spatial;

public class Sphere
{
  const int Segments = 16;
  const int Rings = 12;

  public const uint VertexCount = (Rings + 1) * (Segments + 1);
  public const uint TriangleIndexCount = Rings * Segments * 6;
  public const uint LineIndexCount = (Rings + 1) * Segments * 2 + Rings * (Segments + 1) * 2;

  public float Radius { get; set; } = 1f;
  public Transform Transform { get; set; } = new Transform();

  public Sphere() { }

  public Sphere(Vector3 center, float radius)
  {
    Radius = radius;
    Transform.Position = center;
  }

  public Vector3[] GetVertices()
  {
    var vertices = new Vector3[VertexCount];
    int index = 0;

    for (int i = 0; i <= Rings; i++)
    {
      float phi = i / (float)Rings * MathF.PI;
      float y = MathF.Cos(phi);
      float ringRadius = MathF.Sin(phi);

      for (int j = 0; j <= Segments; j++)
      {
        float theta = j / (float)Segments * MathF.Tau;
        vertices[index++] = new Vector3(ringRadius * MathF.Cos(theta), y, ringRadius * MathF.Sin(theta));
      }
    }

    return vertices;
  }

  public int[] GetTriangleIndices()
  {
    var indices = new int[TriangleIndexCount];
    int index = 0;
    int stride = Segments + 1;

    for (int i = 0; i < Rings; i++)
    {
      for (int j = 0; j < Segments; j++)
      {
        int a = i * stride + j;
        int b = a + stride;

        indices[index++] = a;
        indices[index++] = b;
        indices[index++] = a + 1;

        indices[index++] = a + 1;
        indices[index++] = b;
        indices[index++] = b + 1;
      }
    }

    return indices;
  }

  public int[] GetLineIndices()
  {
    var indices = new int[LineIndexCount];
    int index = 0;
    int stride = Segments + 1;

    for (int i = 0; i <= Rings; i++)
    {
      for (int j = 0; j < Segments; j++)
      {
        int a = i * stride + j;
        indices[index++] = a;
        indices[index++] = a + 1;
      }
    }

    for (int j = 0; j <= Segments; j++)
    {
      for (int i = 0; i < Rings; i++)
      {
        int a = i * stride + j;
        indices[index++] = a;
        indices[index++] = a + stride;
      }
    }

    return indices;
  }

  public Matrix4x4 GetRenderMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Transform.Rotation);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    var radiusScale = Matrix4x4.CreateScale(Radius);
    return radiusScale * scale * rotation * translation;
  }
}
