namespace Hmz.Core.Renderer.Styles;

public record QuadStyle
{
  public Texture2D? Texture { get; init; }
  public Color Color { get; init; } = Color.White;
}
