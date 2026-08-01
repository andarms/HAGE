using System.Numerics;

namespace Hmz.Core.Renderer.OpenGL;

// Pure point-generation math for the procedural 2D shapes — no GL calls, so it's safe to
// reuse from anywhere that needs the same rings (fills, borders, hit-testing, etc.).
static class ShapeGeometry
{
  public static float[] ToVertexData(Vector2[] points)
  {
    float[] vertices = new float[points.Length * 3];
    for (int i = 0; i < points.Length; i++)
    {
      vertices[i * 3 + 0] = points[i].X;
      vertices[i * 3 + 1] = points[i].Y;
      vertices[i * 3 + 2] = 0f;
    }
    return vertices;
  }

  public static float[] ToFanVertexData(Vector2 center, Vector2[] ring)
  {
    float[] vertices = new float[(ring.Length + 2) * 3];
    vertices[0] = center.X;
    vertices[1] = center.Y;
    vertices[2] = 0f;
    for (int i = 0; i < ring.Length; i++)
    {
      vertices[(i + 1) * 3 + 0] = ring[i].X;
      vertices[(i + 1) * 3 + 1] = ring[i].Y;
      vertices[(i + 1) * 3 + 2] = 0f;
    }
    int closing = ring.Length + 1;
    vertices[closing * 3 + 0] = ring[0].X;
    vertices[closing * 3 + 1] = ring[0].Y;
    vertices[closing * 3 + 2] = 0f;
    return vertices;
  }

  public static Vector2[] BuildRectangleRing(float x, float y, float w, float h, float cornerRadius, int cornerSegments = 8)
  {
    if (cornerRadius <= 0f)
    {
      return [new(x, y), new(x + w, y), new(x + w, y + h), new(x, y + h)];
    }

    List<Vector2> points = [];
    AddArc(points, x + w - cornerRadius, y + cornerRadius, cornerRadius, 270, 360, cornerSegments);
    AddArc(points, x + w - cornerRadius, y + h - cornerRadius, cornerRadius, 0, 90, cornerSegments);
    AddArc(points, x + cornerRadius, y + h - cornerRadius, cornerRadius, 90, 180, cornerSegments);
    AddArc(points, x + cornerRadius, y + cornerRadius, cornerRadius, 180, 270, cornerSegments);
    return [.. points];
  }

  public static Vector2[] BuildCircleRing(float centerX, float centerY, float radius, int segments = 32)
  {
    Vector2[] points = new Vector2[segments];
    for (int i = 0; i < segments; i++)
    {
      float angle = i / (float)segments * MathF.Tau;
      points[i] = new Vector2(centerX + radius * MathF.Cos(angle), centerY + radius * MathF.Sin(angle));
    }
    return points;
  }

  static void AddArc(List<Vector2> points, float cx, float cy, float radius, float fromDegrees, float toDegrees, int segments)
  {
    for (int i = 0; i <= segments; i++)
    {
      float degrees = fromDegrees + (toDegrees - fromDegrees) * i / segments;
      float radians = degrees * MathF.PI / 180f;
      points.Add(new Vector2(cx + radius * MathF.Cos(radians), cy + radius * MathF.Sin(radians)));
    }
  }
}
