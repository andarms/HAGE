using Hmz.Core.Renderer._2D;

namespace Hmz.Core.Renderer;

public record TextStyle
{
  public Color Color { get; init; } = Color.White;
  public float FontSize { get; init; } = 12f;
  public Font? Font { get; init; }
  public Stroke? Outline { get; init; }
}
