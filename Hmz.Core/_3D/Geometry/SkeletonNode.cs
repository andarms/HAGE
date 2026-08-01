namespace Hmz.Core._3D.Geometry;

using System.Numerics;

// Mirrors the full Assimp node hierarchy (not just bones) so animation channels, which are
// keyed by node name, can be sampled and composed down through the same parent chain used
// to compute each bone's offset matrix.
public class SkeletonNode
{
  public string Name { get; init; } = "";
  public Matrix4x4 LocalTransform { get; init; } = Matrix4x4.Identity;
  public List<SkeletonNode> Children { get; init; } = [];
}
