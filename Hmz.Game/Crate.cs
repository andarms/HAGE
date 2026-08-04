using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;

namespace Hmz.Game;

public class Crate : GameObject
{
  Cube cube = new()
  {
    Size = new Vector3(1f, 1f, 1f),
  };

  CubeStyle cubeStyle = new()
  {
    Color = Color.Magenta,
  };

  public Crate()
  {
    Transform.Position = new Vector3(-2f, 0f, 2f);
    Collider = new(this)
    {
      Type = CollisionType.Solid,
      Size = new Vector3(1f, 2f, 1f),
      Offset = new Vector3(0f, 1f, 0f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
    };
  }


  public override void Draw()
  {
    Engine.Graphics.DrawCube(cube, WorldMatrix, cubeStyle);
  }
}