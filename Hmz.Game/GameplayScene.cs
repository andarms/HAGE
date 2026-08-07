using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;
using Hmz.Core.Hosting;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Scenes;
using Hmz.Core.Tilemap;

namespace Hmz.Game;

public sealed class GameplayScene : Scene
{
  int displayMode;
  Player.Player player;

  public override void Initialize()
  {
    base.Initialize();
    Engine.MainCamera.AspectRatio = Engine.Viewport.AspectRatio;
    Engine.MainCamera.NearPlane = 0.1f;
    Engine.MainCamera.FarPlane = 1000f;
    Engine.MainCamera.Up = Vector3.UnitY;
    Engine.MainCamera.FieldOfView = MathF.PI / 4f;

    player = new Player.Player();
    Instances.Add(player);
    Tree tree = new();
    Instances.Add(tree);
    Tree tree2 = new()
    {
      Transform = { Position = new Vector3(2f, 0f, 4f), Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4f) },
    };
    Instances.Add(tree2);
    Crate trigger = new();
    Instances.Add(trigger);

    TileMap map = Engine.Content.LoadTilemap("maps/level1.json");
    map.Transform.Position = new Vector3(-22f, 0f, -22f);
    Instances.Add(map);
  }

  public override void Update(float dt)
  {
    base.Update(dt);

    if (Engine.Input.IsActionJustPressed("pause") && Engine.Scenes.Current is not PauseScene)
    {
      Engine.Scenes.Push<PauseScene>();
    }

    if (Engine.Input.IsKeyJustPressed(Hmz.Core.Input.Key.F4))
    {
      displayMode = (displayMode + 1) % 3;
      int width = Engine.Viewport.LogicalWidth;
      int height = Engine.Viewport.LogicalHeight;
      switch (displayMode)
      {
        case 0:
          Engine.SetWindowedSize(width, height);
          break;
        case 1:
          Engine.SetWindowedSize(width * 2, height * 2);
          break;
        case 2:
          Engine.FullScreen();
          break;
      }
    }
  }

  public override void Draw()
  {
    Engine.Graphics.StartMode3D(Engine.MainCamera);
    base.Draw();
    Engine.Graphics.DrawDebugGrid(40, 40, 1f);
    Engine.Graphics.EndMode3D();

    Engine.Graphics.DrawText($"FPS: {Performance.FPS} | Draws: {Engine.Graphics.DrawCallCount}", 10f, 10f, new TextStyle
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
