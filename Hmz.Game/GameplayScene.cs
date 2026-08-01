using System.Numerics;
using Hmz.Core;
using Hmz.Core.Content;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer._2D;
using Hmz.Core.Renderer._3D;
using Hmz.Core.Scenes;

namespace Hmz.Game;

public sealed class GameplayScene : Scene
{
  readonly Camera3D camera = new()
  {
    Position = new(0f, 8f, 9f),
    Target = Vector3.Zero,
    Up = Vector3.UnitY,
    FieldOfView = MathF.PI / 4f,
  };

  public override void Initialize()
  {
    base.Initialize();
    Player player = new();
    Add(player);
  }

  public override void Update(float dt)
  {
    base.Update(dt);
    camera.Orbit(Vector3.Zero, 0.5f * dt, 0f);
  }

  public override void Draw()
  {
    Engine.Graphics.StartMode3D(camera);
    base.Draw();
    Engine.Graphics.EndMode3D();

    Engine.Graphics.DrawText($"FPS: {Performance.FPS}", 10f, 10f, new TextStyle
    {
      Color = Color.White,
      FontSize = 32f,
      Outline = new Stroke { Color = Color.Black, Width = 2f },
    });
  }

  public override void Terminate()
  {
    Engine.Content.UnloadAll();
    base.Terminate();
  }
}


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