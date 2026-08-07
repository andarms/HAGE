using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Tilemap;

namespace Hmz.Game;

public class Wall : Tile3D
{
  readonly Cube cube = new()
  {
    Size = new Vector3(2f, 2f, 1f),
  };

  readonly CubeStyle cubeStyle = new()
  {
    Color = Color.Gray,
  };

  public Wall()
  {
    Size = TileSize.OneByOne;
    Collider = new(this)
    {
      Type = CollisionType.Solid,
      Size = new Vector3(2f, 2f, 1f),
      Offset = new Vector3(0f, 1f, 0f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
    };
  }

  // The grid sets this tile's Y position to the floor level.
  // The cube mesh has its center at the origin.
  // This draws the cube shifted up, to match the collider offset.
  public override void Draw()
  {
    Matrix4x4 pivotMatrix = Matrix4x4.CreateTranslation(0f, 1f, 0f) * WorldMatrix;
    Engine.Graphics.DrawCube(cube, pivotMatrix, cubeStyle);
  }
}
