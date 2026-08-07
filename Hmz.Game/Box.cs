using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Tilemap;

namespace Hmz.Game;

public abstract class Box : Tile3D
{
  const float CellSize = 2f;

  readonly Cube cube;
  readonly CubeStyle cubeStyle = new()
  {
    Color = Color.Gray,
  };

  protected Box(TileSize size)
  {
    Size = size;
    (int width, int depth) = size.Dimensions();
    Vector3 boxSize = new(width * CellSize, CellSize, depth * CellSize);

    cube = new Cube { Size = boxSize };
    Collider = new(this)
    {
      Type = CollisionType.Solid,
      Size = boxSize,
      Offset = new Vector3(0f, boxSize.Y / 2f, 0f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
    };
  }

  // The grid sets this tile's Y position to the floor level.
  // The cube mesh has its center at the origin.
  // This draws the cube shifted up, to match the collider offset.
  public override void Draw()
  {
    Matrix4x4 pivotMatrix = Matrix4x4.CreateTranslation(0f, cube.Size.Y / 2f, 0f) * WorldMatrix;
    Engine.Graphics.DrawCube(cube, pivotMatrix, cubeStyle);
  }
}

public class Box1x1 : Box
{
  public Box1x1() : base(TileSize.OneByOne) { }
}

public class Box1x2 : Box
{
  public Box1x2() : base(TileSize.OneByTwo) { }
}

public class Box1x3 : Box
{
  public Box1x3() : base(TileSize.OneByThree) { }
}

public class Box2x1 : Box
{
  public Box2x1() : base(TileSize.TwoByOne) { }
}

public class Box2x2 : Box
{
  public Box2x2() : base(TileSize.TwoByTwo) { }
}

public class Box2x3 : Box
{
  public Box2x3() : base(TileSize.TwoByThree) { }
}

public class Box3x1 : Box
{
  public Box3x1() : base(TileSize.ThreeByOne) { }
}

public class Box3x2 : Box
{
  public Box3x2() : base(TileSize.ThreeByTwo) { }
}

public class Box3x3 : Box
{
  public Box3x3() : base(TileSize.ThreeByThree) { }
}
