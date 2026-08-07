using System.Numerics;

namespace Hmz.Core._3D;

public readonly struct Ray(Vector3 origin, Vector3 direction)
{
  public Vector3 Origin { get; } = origin;
  public Vector3 Direction { get; } = direction;

  public Vector3? IntersectHorizontalPlane(float planeY)
  {
    if (MathF.Abs(Direction.Y) < 1e-6f) return null;

    float t = (planeY - Origin.Y) / Direction.Y;
    if (t < 0f) return null;

    return Origin + Direction * t;
  }
}
