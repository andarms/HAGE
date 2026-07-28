namespace Hmz.Core.Renderer._2D;

public sealed class Texture2D(uint handle, int width, int height) : IDisposable
{
  public int Width { get; } = width;
  public int Height { get; } = height;
  internal uint Handle { get; } = handle;     // the GL texture id — hidden behind your wall
  public void Dispose() { }        // glDeleteTexture
}