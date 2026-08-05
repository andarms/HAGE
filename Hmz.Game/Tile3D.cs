using System.Numerics;
using Hmz.Core.GOM;

namespace Hmz.Game;

public class Tile3D : GameObject
{
  const float RightAngle = MathF.PI / 2f;

  int rotationSteps;
  Vector3? baseColliderSize;
  Vector3? baseColliderOffset;

  // Snaps to the nearest 90-degree step around the up axis. Collider bounds are a plain
  // AABB (see Collider.Bounds), so anything other than a quarter turn can't be represented -
  // a quarter turn just swaps the X/Z extents.
  public void RotateTile(float angle)
  {
    rotationSteps = ((int)MathF.Round(angle / RightAngle) % 4 + 4) % 4;
    Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotationSteps * RightAngle);

    if (Collider is not { } collider) return;

    baseColliderSize ??= collider.Size;
    baseColliderOffset ??= collider.Offset;

    bool swapAxes = rotationSteps % 2 != 0;
    collider.Size = swapAxes ? SwapXZ(baseColliderSize.Value) : baseColliderSize.Value;
    collider.Offset = swapAxes ? SwapXZ(baseColliderOffset.Value) : baseColliderOffset.Value;
  }

  static Vector3 SwapXZ(Vector3 v) => new(v.Z, v.Y, v.X);
}
