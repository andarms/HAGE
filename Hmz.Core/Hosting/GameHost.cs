using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

using Hmz.Core.Renderer;
using Hmz.Core.Renderer.OpenGL;
using ImGuiNET;

namespace Hmz.Core.Hosting;

// Owns the window, the platform/fullscreen plumbing, and the render loop -
// none of which the Game it hosts needs to know anything about.
public class GameHost
{
  readonly Game game;
  readonly IWindow window;
  readonly WindowController windowController;
  IInputContext input;
  ImGuiController imGuiController;
  public IGraphics Graphics;
  readonly Viewport viewport;
  bool isClosed;
  bool closeRequested;
  bool windowShown;

  public Viewport Viewport => viewport;

  public GameHost(Game game, GameOptions? options = null, IPlatformWindowService? platform = null)
  {
    this.game = game;

    options ??= new GameOptions();
    viewport = new Viewport(options.Width, options.Height);

    windowController = new WindowController(options, platform);
    window = windowController.Window;

    window.Load += BaseInitialize;
    window.Update += BaseUpdate;
    window.Render += BaseRender;
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

  public void FullScreen()
  {
    windowController.FullScreen();
    ResizeViewport(window.FramebufferSize);
  }

  public void SetWindowedSize(int width, int height)
  {
    windowController.SetWindowedSize(width, height);
    ResizeViewport(window.FramebufferSize);
  }

  private void BaseInitialize()
  {
    input = window.CreateInput();
    for (int i = 0; i < input.Keyboards.Count; i++)
    {
      input.Keyboards[i].KeyDown += KeyDown;
    }
    Engine.Input.Initialize(input);

    Engine.GL = GL.GetApi(window);
    Graphics = new OpenGLGraphics(viewport.LogicalWidth, viewport.LogicalHeight);

    game.LoadScenes();

    imGuiController = new ImGuiController(Engine.GL, window, input);
    ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

    ResizeViewport(window.FramebufferSize);

    game.Initialize();
  }

  void ResizeViewport(Vector2D<int> size)
  {
    viewport.Resize(size.X, size.Y);
    Engine.GL.Viewport(viewport.X, viewport.Y, (uint)viewport.Width, (uint)viewport.Height);
  }

  private void BaseUpdate(double delta)
  {
    GameTime.DeltaTime = (float)delta;
    GameTime.TotalTime += (float)delta;

    if (closeRequested)
    {
      window.Close();
      return;
    }

    game.Update((float)delta);
  }

  private void BaseRender(double delta)
  {
    Performance.FrameTimeMs = (float)(delta * 1000.0);
    Performance.FPS = delta > 0 ? (int)Math.Round(1.0 / delta) : 0;

    // Fullscreen and windowed transitions can briefly report a zero framebuffer.
    // Refresh here so the first valid size after the transition is applied.
    ResizeViewport(window.FramebufferSize);

    imGuiController.Update((float)delta);
    ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

    Engine.GL.Viewport(0, 0, (uint)viewport.FramebufferWidth, (uint)viewport.FramebufferHeight);
    Engine.GL.ClearColor(0f, 0f, 0f, 1f);
    Engine.GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

    // glClear ignores the viewport, so scissor the background clear to the fitted area.
    Engine.GL.Enable(EnableCap.ScissorTest);
    Engine.GL.Scissor(viewport.X, viewport.Y, (uint)viewport.Width, (uint)viewport.Height);
    Graphics.Clear(game.BackgroundColor);
    Engine.GL.Disable(EnableCap.ScissorTest);

    Engine.GL.Viewport(viewport.X, viewport.Y, (uint)viewport.Width, (uint)viewport.Height);
    Graphics.StartFrame();

    game.Draw();

    Graphics.EndFrame();

    Engine.GL.Viewport(0, 0, (uint)viewport.FramebufferWidth, (uint)viewport.FramebufferHeight);
    imGuiController.Render();

    var err = Engine.GL.GetError();
    if (err != GLEnum.NoError)
    {
      Console.WriteLine($"GL Error: {err}");
    }

    // Deferred from window creation (see WindowController) until content has
    // actually been rendered, so we swap in a finished frame instead of
    // showing a blank OS-drawn window while LoadScenes/Initialize are busy.
    if (!windowShown)
    {
      windowShown = true;
      window.IsVisible = true;
    }
  }

  private void OnFramebufferResize(Vector2D<int> newSize)
  {
    ResizeViewport(newSize);
  }

  private void OnClose()
  {
    if (isClosed)
    {
      return;
    }

    isClosed = true;
    windowController.RestoreIfFullscreen();
    game.Terminate();

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
