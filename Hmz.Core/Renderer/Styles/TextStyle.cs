using Hmz.Core.Renderer;

namespace Hmz.Core.Renderer.Styles;

public record TextStyle
{
  public Color Color { get; init; } = Color.White;
  public float FontSize { get; init; } = 12f;
  public Font? Font { get; init; }
  public Stroke? Outline { get; init; }
}
