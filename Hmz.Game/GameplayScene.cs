using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;
using Hmz.Core.Hosting;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
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
  int displayMode;

  public override void Initialize()
  {
    base.Initialize();
    camera.AspectRatio = Engine.Viewport.AspectRatio;
    Player player = new();
    Instances.Add(player);
  }

  public override void Update(float dt)
  {
    base.Update(dt);

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
