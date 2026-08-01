using System.Runtime.InteropServices;
using Hmz.Core;

namespace Hmz.Windows;

public sealed class WindowsPlatformWindowService : IPlatformWindowService
{
  public void SetTaskbarVisible(bool visible)
  {
    nint taskbar = NativeMethods.FindWindow("Shell_TrayWnd", null);
    if (taskbar != 0)
    {
      // SW_SHOW activates the window it's showing, which would steal focus
      // from the game the moment we leave fullscreen. SW_SHOWNA shows it
      // without touching activation/focus.
      NativeMethods.ShowWindow(taskbar, visible ? NativeMethods.SW_SHOWNA : NativeMethods.SW_HIDE);
    }
  }

  static class NativeMethods
  {
    public const int SW_HIDE = 0;
    public const int SW_SHOWNA = 8;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern nint FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(nint hWnd, int nCmdShow);
  }
}
