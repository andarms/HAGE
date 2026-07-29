using Hmz.Core.Renderer;
using Silk.NET.OpenGL;

namespace Hmz.Core;

public static class Engine
{
  internal static Game Current { get; set; }
  internal static GL GL;

  public static IGraphics Graphics => Current.Graphics;
}