namespace Hmz.Core.Renderer;

public sealed class Viewport
{
  readonly int logicalWidth;
  readonly int logicalHeight;

  public int LogicalWidth => logicalWidth;
  public int LogicalHeight => logicalHeight;
  public int FramebufferWidth { get; private set; }
  public int FramebufferHeight { get; private set; }
  public int X { get; private set; }
  public int Y { get; private set; }
  public int Width { get; private set; }
  public int Height { get; private set; }
  public float AspectRatio => logicalWidth / (float)logicalHeight;

  public Viewport(int logicalWidth, int logicalHeight)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalHeight);

    this.logicalWidth = logicalWidth;
    this.logicalHeight = logicalHeight;
    Resize(logicalWidth, logicalHeight);
  }

  public void Resize(int framebufferWidth, int framebufferHeight)
  {
    if (framebufferWidth <= 0 || framebufferHeight <= 0)
    {
      return;
    }

    FramebufferWidth = Math.Max(1, framebufferWidth);
    FramebufferHeight = Math.Max(1, framebufferHeight);

    float scale = MathF.Min(
      FramebufferWidth / (float)logicalWidth,
      FramebufferHeight / (float)logicalHeight);
    Width = Math.Max(1, (int)MathF.Round(logicalWidth * scale));
    Height = Math.Max(1, (int)MathF.Round(logicalHeight * scale));
    X = (FramebufferWidth - Width) / 2;
    Y = (FramebufferHeight - Height) / 2;
  }
}