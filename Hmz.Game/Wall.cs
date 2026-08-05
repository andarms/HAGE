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
    // The cube is 2 (X) x 1 (Z) world units, and 1 grid cell is placed per wall segment
    // (matching cellSize to the cube's width) - not a 2x2-cell footprint.
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

  // Grid placement gives this tile's Y as the floor level (Section 2.3), but the cube mesh
  // is centered on its origin, so it's drawn shifted up by the same offset as the collider.
  public override void Draw()
  {
    Matrix4x4 pivotMatrix = Matrix4x4.CreateTranslation(0f, 1f, 0f) * WorldMatrix;
    Engine.Graphics.DrawCube(cube, pivotMatrix, cubeStyle);
  }
}
