using System.Numerics;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;
using Hmz.Core.Tilemap;

namespace Hmz.Game;

public class Slope : Tile3D
{
  const float CellSize = 2f;
  const float WallThickness = 0.1f;

  public float RiseHeight { get; }

  float width;
  float depth;
  List<SlopeWall> walls = [];

  public Slope(TileSize size, float riseHeight)
  {
    Size = size;
    RiseHeight = riseHeight;

    (int cellsWidth, int cellsDepth) = size.Dimensions();
    width = cellsWidth * CellSize;
    depth = cellsDepth * CellSize;

    Collider = new(this)
    {
      Type = CollisionType.Trigger,
      Size = new Vector3(width, riseHeight, depth),
      Offset = new Vector3(0f, riseHeight / 2f, 0f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
      GroundHeight = GetGroundHeight,
    };
  }

  // The low and high ends (local Z) stay open to climb through and step off onto whatever
  // follows. The two side edges get solid rails so walking/falling off the sides is blocked.
  protected override void OnInitialize()
  {
    Vector3 wallOffset = new(0f, RiseHeight / 2f, 0f);

    walls = [
      new SlopeWall(new Vector3(-width / 2f, 0f, 0f), new Vector3(WallThickness, RiseHeight, depth), wallOffset),
      new SlopeWall(new Vector3(width / 2f, 0f, 0f), new Vector3(WallThickness, RiseHeight, depth), wallOffset),
    ];

    foreach (SlopeWall wall in walls)
    {
      wall.ApplyAxesSwap(AxesSwapped);
      Children.Add(wall);
    }
  }

  public override void RotateTile(float angle)
  {
    base.RotateTile(angle);

    foreach (SlopeWall wall in walls)
    {
      wall.ApplyAxesSwap(AxesSwapped);
    }
  }

  // The low edge sits at local Z = -depth / 2, the high edge at local Z = depth / 2, matching
  // how tiles are placed by their footprint center rather than a corner. width/depth stay in
  // this un-rotated local frame; the inverse WorldMatrix below already accounts for rotation.
  float GetGroundHeight(Vector3 worldPosition)
  {
    if (!Matrix4x4.Invert(WorldMatrix, out Matrix4x4 inverse))
    {
      return Transform.Position.Y;
    }

    Vector3 local = Vector3.Transform(worldPosition, inverse);
    float t = Math.Clamp(local.Z / depth + 0.5f, 0f, 1f);

    return Transform.Position.Y + t * RiseHeight;
  }
}

class SlopeWall : GameObject
{
  readonly Vector3 baseSize;
  readonly Vector3 baseOffset;

  public SlopeWall(Vector3 localPosition, Vector3 size, Vector3 offset)
  {
    Transform.Position = localPosition;

    baseSize = size;
    baseOffset = offset;

    Collider = new(this)
    {
      Type = CollisionType.Solid,
      Size = baseSize,
      Offset = baseOffset,
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
    };
  }

  // Position rotates correctly through the parent's WorldMatrix already; only the AABB
  // extents (which Collider.Bounds never rotates) need the X/Z swap.
  public void ApplyAxesSwap(bool swapAxes)
  {
    if (Collider is not { } collider)
    {
      return;
    }

    collider.Size = swapAxes ? SwapXZ(baseSize) : baseSize;
    collider.Offset = swapAxes ? SwapXZ(baseOffset) : baseOffset;
  }

  static Vector3 SwapXZ(Vector3 v) => new(v.Z, v.Y, v.X);
}
