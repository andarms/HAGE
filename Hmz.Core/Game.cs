using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

using Hmz.Core.Renderer;
using Hmz.Core.Renderer.OpenGL;
using Hmz.Core.Scenes;
using ImGuiNET;

namespace Hmz.Core;

public record GameOptions
{
  public int Width { get; init; } = 800;
  public int Height { get; init; } = 600;
  public string Title { get; init; } = "Hamaze";
  public int TargetFps { get; init; } = 60;
}

public class Game
{
  readonly IWindow window;
  IInputContext input;
  ImGuiController imGuiController;
  public IGraphics Graphics;
  private int width;
  private int height;
  readonly Scene startupScene;
  bool isClosed;
  bool closeRequested;

  public Game(Scene startupScene, GameOptions? options = null)
  {
    ArgumentNullException.ThrowIfNull(startupScene);
    this.startupScene = startupScene;

    options ??= new GameOptions();
    width = options.Width;
    height = options.Height;

    WindowOptions windowOptions = WindowOptions.Default;
    windowOptions.Size = new Vector2D<int>(options.Width, options.Height);
    windowOptions.Title = options.Title;
    windowOptions.FramesPerSecond = options.TargetFps;
    windowOptions.API = new GraphicsAPI(
      ContextAPI.OpenGL,
      ContextProfile.Core,
      ContextFlags.ForwardCompatible,
      new APIVersion(3, 3)
    );

    window = Window.Create(windowOptions);

    window.Load += Initialize;
    window.Update += Update;
    window.Render += Render;
    window.FramebufferResize += OnFramebufferResize;
    window.Closing += OnClose;
  }

  public void Run()
  {
    Engine.Current = this;
    try
    {
      window.Run();
    }
    finally
    {
      OnClose();
      window.Dispose();
    }
  }

  private void Initialize()
  {
    input = window.CreateInput();
    for (int i = 0; i < input.Keyboards.Count; i++)
    {
      input.Keyboards[i].KeyDown += KeyDown;
    }
    Engine.Input.Initialize(input);

    Engine.GL = GL.GetApi(window);
    Graphics = new OpenGLGraphics(width, height);

    Engine.Scenes.Add(startupScene);
    Engine.Initialize(this, Engine.GL);

    imGuiController = new ImGuiController(Engine.GL, window, input);
    ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

    window.FramebufferResize += ResizeViewport;

  }

  void ResizeViewport(Vector2D<int> size)
  {
    width = size.X;
    height = size.Y;
    Engine.GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    Graphics.Resize(size.X, size.Y);
  }

  private void Update(double delta)
  {
    GameTime.DeltaTime = (float)delta;
    GameTime.TotalTime += (float)delta;

    if (closeRequested)
    {
      window.Close();
      return;
    }

    Engine.Update((float)delta);
  }

  private void Render(double delta)
  {
    Performance.FrameTimeMs = (float)(delta * 1000.0);
    Performance.FPS = delta > 0 ? (int)Math.Round(1.0 / delta) : 0;

    imGuiController.Update((float)delta);
    ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);


    Graphics.Clear(Color.CornflowerBlue);
    Graphics.StartFrame();

    Engine.Draw();

    Graphics.EndFrame();

    imGuiController.Render();

    var err = Engine.GL.GetError();
    if (err != GLEnum.NoError)
    {
      Console.WriteLine($"GL Error: {err}");
    }
  }

  private void OnFramebufferResize(Vector2D<int> newSize)
  {
    Engine.GL.Viewport(newSize);
    Graphics.Resize(newSize.X, newSize.Y);
  }

  private void OnClose()
  {
    if (isClosed)
    {
      return;
    }

    isClosed = true;
    Engine.Scenes.Terminate();

    imGuiController.Dispose();
    // ImGuiController doesn't unhook the GLFW key/mouse callbacks it registers,
    // so any further input event would invoke a dangling callback into the
    // now-destroyed ImGui context and crash with an access violation.
    // Disposing the input context unregisters those GLFW callbacks. This is
    // also why window.Close() must never be called from inside an input
    // event handler (see KeyDown): Closing runs synchronously off the same
    // call stack, so a later subscriber on that same key event (like
    // ImGuiController) would still fire into the resources we just disposed.
    input.Dispose();
    Graphics.Dispose();
  }

  private void KeyDown(IKeyboard keyboard, Key key, int scancode)
  {
    if (key == Key.Escape)
    {
      // Deferred to Update(): calling window.Close() here would run Closing
      // (and dispose ImGuiController) synchronously inside this same GLFW
      // key-event dispatch, crashing any subscriber still queued after us
      // on the same event (see OnClose).
      closeRequested = true;
    }
  }
}

