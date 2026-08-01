using System.Numerics;
using Hmz.Core;
using Hmz.Core.Renderer._3D;
using Hmz.Core.Scenes;

namespace Hmz.Game;

public class Player : GameObject
{
  Model model;

  public Player()
  {
    Transform.Position = new Vector3(0f, 0f, 0f);
    Transform.Scale = new Vector3(1f, 1f, 1f);
  }


  public override void Initialize()
  {
    model = Engine.Content.LoadModel("models/player.gltf");
    model.Transform.Position = Transform.Position;
    model.Transform.Scale = Transform.Scale;
    model.Play("walk");
  }

  public override void Update(float dt)
  {
    model.Update(dt);
  }

  public override void Draw()
  {
    Engine.Graphics.DrawModel(model);
  }
}