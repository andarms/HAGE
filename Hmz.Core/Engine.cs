using Hmz.Core.Content;
using Hmz.Core.Hosting;
using Hmz.Core.Input;
using Hmz.Core.Renderer;
using Hmz.Core.Scenes;
using Silk.NET.OpenGL;

namespace Hmz.Core;

public static class Engine
{
  internal static GameHost Current { get; set; }
  internal static GL GL;

  public static IGraphics Graphics => Current.Graphics;
  public static Viewport Viewport => Current.Viewport;
  public static void FullScreen() => Current.FullScreen();
  public static void SetWindowedSize(int width, int height) => Current.SetWindowedSize(width, height);
  public static InputManager Input { get; } = new();
  public static SceneManager Scenes { get; } = new();
  public static ContentManager Content = new();
  public static bool DebugMode { get; set; } = false;

  public static void Initialize()
  {
    Scenes.Initialize();
  }

  public static void Update(float deltaTime)
  {
    Scenes.Update(deltaTime);
    Input.EndFrame();
  }

  public static void Draw()
  {
    Scenes.Draw();
  }

  public static void Terminate()
  {
    Scenes.Terminate();
  }
}