using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Hmz.Core.Hosting;

sealed class WindowController
{
  readonly IWindow window;
  readonly IPlatformWindowService? platform;
  bool isFullscreen;

  public IWindow Window => window;

  public WindowController(GameOptions options, IPlatformWindowService? platform)
  {
    this.platform = platform;

    WindowOptions windowOptions = WindowOptions.Default;
    windowOptions.Size = new Vector2D<int>(options.Width, options.Height);
    windowOptions.Title = options.Title;
    // VSync defaults to true and locks the swap interval to the display's refresh rate,
    // silently overriding FramesPerSecond regardless of its value. Disable it so TargetFps
    // actually governs the loop.
    windowOptions.VSync = false;
    windowOptions.FramesPerSecond = options.TargetFps;
    windowOptions.UpdatesPerSecond = options.TargetFps;
    windowOptions.API = new GraphicsAPI(
      ContextAPI.OpenGL,
      ContextProfile.Core,
      ContextFlags.ForwardCompatible,
      new APIVersion(3, 3)
    );
    // Hidden until GameHost has finished loading and rendered its first frame -
    // otherwise the OS shows a blank/white window for however long that takes.
    windowOptions.IsVisible = false;

    window = Silk.NET.Windowing.Window.Create(windowOptions);
  }

  public void FullScreen()
  {
    if (isFullscreen)
    {
      return;
    }
    isFullscreen = true;

    // WindowState.Fullscreen asks GLFW to attach the window to a monitor
    // (glfwSetWindowMonitor), which triggers a real display mode switch and
    // blanks the screen black for a frame or two. Borderless "windowed
    // fullscreen" instead just resizes/repositions a normal window to cover
    // the monitor, so the compositor never blanks the display.
    var monitor = window.Monitor ?? Silk.NET.Windowing.Monitor.GetMainMonitor(window);
    window.WindowBorder = WindowBorder.Hidden;
    window.Position = monitor.Bounds.Origin;
    // IMonitor.Bounds is GLFW's work area (screen minus taskbar), not the
    // full monitor resolution, so sizing to it would leave a taskbar-sized
    // gap of bare desktop once the taskbar itself is hidden below. The full
    // physical resolution instead comes from VideoMode.
    var resolution = monitor.VideoMode.Resolution ?? monitor.Bounds.Size;
    // A borderless window sized to *exactly* match the monitor gets treated
    // by Windows as a real fullscreen app, which brings back the same
    // display flip-mode blink we moved off WindowState.Fullscreen to avoid
    // (it's also why the taskbar briefly tried to auto-hide on its own).
    // One extra pixel keeps it just short of an exact match - imperceptible,
    // since we already hide the taskbar ourselves and don't need Windows to
    // detect this as fullscreen.
    window.Size = new Vector2D<int>(resolution.X + 1, resolution.Y + 1);
    platform?.SetTaskbarVisible(false);
  }

  public void SetWindowedSize(int width, int height)
  {
    if (isFullscreen)
    {
      platform?.SetTaskbarVisible(true);
    }
    isFullscreen = false;
    window.WindowBorder = WindowBorder.Resizable;
    window.Size = new Vector2D<int>(width, height);
    window.Center();
  }

  public void RestoreIfFullscreen()
  {
    if (isFullscreen)
    {
      platform?.SetTaskbarVisible(true);
    }
  }
}
