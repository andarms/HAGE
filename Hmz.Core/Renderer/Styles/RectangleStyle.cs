namespace Hmz.Core.Renderer;

public record RectangleStyle { public Color? Fill { get; init; } = Color.White; public Stroke? Border { get; init; } public float CornerRadius { get; init; } }
