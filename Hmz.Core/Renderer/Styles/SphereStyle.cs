using Hmz.Core.Renderer;

namespace Hmz.Core.Renderer.Styles;

public record SphereStyle
{
  public Color Color { get; init; } = Color.White;
  public float Width { get; init; } = 1f;
  public bool Wireframe { get; init; } = false;
  public Stroke? Border { get; init; } = new Stroke { Color = Color.Black, Width = 1f };
}
