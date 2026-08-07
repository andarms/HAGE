using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;

namespace Hmz.Core.UI;

public class ColorRect : Container
{
  public RectangleStyle Style { get; set; } = new() { Fill = Color.White };

  public ColorRect()
  {
    FillParent();
  }

  protected override void OnDraw()
  {
    Engine.Graphics.DrawRectangle(Bounds, Style);
    FlushBackground();
  }
}
