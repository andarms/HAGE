using System.Numerics;
using Hmz.Core.GOM;

namespace Hmz.Core.Tilemap;

public enum TileSize
{
  OneByOne,
  OneByTwo,
  OneByThree,
  TwoByOne,
  TwoByTwo,
  TwoByThree,
  ThreeByOne,
  ThreeByTwo,
  ThreeByThree,
}

public class Tile3D : GameObject
{
  const float RightAngle = MathF.PI / 2f;

  public TileSize Size { get; set; } = TileSize.OneByOne;

  int rotationSteps;
  Vector3? baseColliderSize;
  Vector3? baseColliderOffset;

  // True on a quarter and three-quarter turn, when a subclass with its own extra colliders
  // (built from this tile's un-rotated Size/Offset) needs to swap their X/Z extents too.
  protected bool AxesSwapped => rotationSteps % 2 != 0;

  // Snaps to the nearest 90-degree step around the up axis. Collider bounds are a plain
  // AABB (see Collider.Bounds), so anything other than a quarter turn can't be represented -
  // a quarter turn just swaps the X/Z extents.
  public virtual void RotateTile(float angle)
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
