using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;

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
    Size = TileSize.TwoByTwo;
    Collider = new(this)
    {
      Type = CollisionType.Solid,
      Size = new Vector3(2f, 2f, 1f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
    };
  }


  public override void Draw()
  {
    Engine.Graphics.DrawCube(cube, WorldMatrix, cubeStyle);
  }
}
