using Hmz.Core.Renderer.Styles;
using System.Numerics;

namespace Hmz.Core.UI;

public class Label : UIElement
{
  public string Text { get; set; } = string.Empty;
  public TextStyle Style { get; set; } = new();

  public override Vector2 Measure(Vector2 availableSize)
  {
    DesiredSize = ExplicitSize ?? Engine.Graphics.MeasureText(Text, Style);
    return DesiredSize;
  }

  protected override void DrawSelf()
  {
    Engine.Graphics.DrawText(Text, Bounds.X, Bounds.Y, Style);
  }
}
