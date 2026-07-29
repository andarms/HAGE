namespace Hmz.Core.Renderer._2D;

using FontStashSharp;

// Wraps FontStashSharp's FontSystem so game code never touches FSS types directly —
// same pattern as Texture2D hiding its GL handle.
public sealed class Font(FontSystem system) : IDisposable
{
  internal FontSystem System { get; } = system;
  public void Dispose() => System.Dispose();
}
