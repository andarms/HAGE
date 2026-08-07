using Hmz.Core._3D;
using Hmz.Core.Collisions;
using Hmz.Core.Content;
using Hmz.Core.Hosting;
using Hmz.Core.Input;
using Hmz.Core.Renderer;
using Hmz.Core.Scenes;
using Hmz.Core.Tilemap;
using Silk.NET.OpenGL;

namespace Hmz.Core;

public static class Engine
{
  internal static GameHost Current { get; set; }
  internal static GL GL;

  // Simulation step for Scenes.Update/collision resolution, run through the accumulator in
  // Update below so gameplay and physics behave the same regardless of render frame rate.
  public const float FixedDeltaTime = 1f / 60f;
  const int MaxFixedStepsPerUpdate = 5;

  static float accumulator;

  public static IGraphics Graphics => Current.Graphics;
  public static Viewport Viewport => Current.Viewport;
  public static void FullScreen() => Current.FullScreen();
  public static void SetWindowedSize(int width, int height) => Current.SetWindowedSize(width, height);
  public static InputManager Input { get; } = new();
  public static SceneManager Scenes { get; } = new();
  public static ContentManager Content { get; } = new();
  public static CollisionsManager Collisions { get; } = new();
  public static bool DebugMode { get; set; } = false;
  public static Camera3D MainCamera { get; set; } = new Camera3D();
  public static GameObjectRegistry GameObjectRegistry { get; } = new();

  public static void Initialize()
  {
    Scenes.Initialize();
  }

  public static void Update(float deltaTime)
  {
    accumulator += deltaTime;

    int steps = 0;
    while (accumulator >= FixedDeltaTime && steps < MaxFixedStepsPerUpdate)
    {
      if (Input.IsKeyJustPressed(Key.F1))
      {
        DebugMode = !DebugMode;
      }

      Scenes.Update(FixedDeltaTime);
      Collisions.UpdateCollisions();
      Input.EndFrame();
      accumulator -= FixedDeltaTime;
      steps++;
    }

    if (steps == MaxFixedStepsPerUpdate)
    {
      accumulator = 0f;
    }
  }

  public static void Draw()
  {
    Scenes.Draw();
    Scenes.DrawCanvas();
  }

  public static void Terminate()
  {
    Scenes.Terminate();
  }
}