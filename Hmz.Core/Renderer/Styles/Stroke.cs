using Hmz.Core.Renderer;

namespace Hmz.Core.Renderer.Styles;

public record Stroke { public Color Color { get; init; } = Color.Black; public float Width { get; init; } = 1f; }
