using System.Numerics;
using Hmz.Core._3D.Geometry;

namespace Hmz.Core.Collisions;

public readonly record struct CollisionBox(Vector3 Min, Vector3 Max)
{
  public bool Intersects(CollisionBox other)
  {
    return Min.X <= other.Max.X && Max.X >= other.Min.X &&
    Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
    Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
  }

  public Cube ToCube()
  {
    Vector3 size = Max - Min;
    return new Cube(Min, size.X, size.Y, size.Z);
  }

}
