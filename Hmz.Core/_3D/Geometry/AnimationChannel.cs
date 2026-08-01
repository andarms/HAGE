namespace Hmz.Core._3D.Geometry;

using System.Numerics;

public class AnimationChannel
{
  public List<(float Time, Vector3 Value)> Positions { get; init; } = [];
  public List<(float Time, Quaternion Value)> Rotations { get; init; } = [];
  public List<(float Time, Vector3 Value)> Scales { get; init; } = [];
}
