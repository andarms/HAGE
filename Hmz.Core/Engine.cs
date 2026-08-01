using Hmz.Core.Content;
using Hmz.Core.Input;
using Hmz.Core.Renderer;
using Hmz.Core.Scenes;
using Silk.NET.OpenGL;

namespace Hmz.Core;

public static class Engine
{
  internal static Game Current { get; set; }
  internal static GL GL;

  public static IGraphics Graphics => Current.Graphics;
  public static InputManager Input { get; } = new();
  public static SceneManager Scenes { get; } = new();
  public static ContentManager Content = new();
  public static bool DebugMode { get; set; } = false;

  public static void Initialize(Game game, GL gl)
  {
    GL = gl;
    Current = game;
    Scenes.Initialize();
  }

  public static void Update(float deltaTime)
  {
    Input.EndFrame();
    Scenes.Update(deltaTime);
  }

  public static void Draw()
  {
    Scenes.Draw();
  }

}