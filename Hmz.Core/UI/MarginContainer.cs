using Hmz.Core._2D;
using System.Numerics;

namespace Hmz.Core.UI;

public class MarginContainer : Container
{
  public float MarginLeft { get; set; }
  public float MarginTop { get; set; }
  public float MarginRight { get; set; }
  public float MarginBottom { get; set; }

  public void SetMargin(float all)
  {
    MarginLeft = all;
    MarginTop = all;
    MarginRight = all;
    MarginBottom = all;
  }

  public override Vector2 Measure(Vector2 availableSize)
  {
    Vector2 inset = new(MarginLeft + MarginRight, MarginTop + MarginBottom);
    Vector2 desired = Vector2.Zero;
    foreach (UIElement child in Children.OfType<UIElement>())
    {
      desired = child.Measure(availableSize - inset);
    }
    DesiredSize = ExplicitSize ?? desired + inset;
    return DesiredSize;
  }

  public override void Arrange(Rectangle finalRect)
  {
    Bounds = ResolveOwnBounds(finalRect);

    Rectangle inner = new(
      Bounds.X + MarginLeft,
      Bounds.Y + MarginTop,
      Bounds.Width - MarginLeft - MarginRight,
      Bounds.Height - MarginTop - MarginBottom);

    foreach (UIElement child in Children.OfType<UIElement>())
    {
      child.Arrange(inner);
    }
  }
}
