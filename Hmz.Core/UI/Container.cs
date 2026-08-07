using Hmz.Core._2D;
using System.Numerics;

namespace Hmz.Core.UI;

public class Container : UIElement
{
  public override Vector2 Measure(Vector2 availableSize)
  {
    Vector2 desired = Vector2.Zero;
    foreach (UIElement child in Children.OfType<UIElement>())
    {
      Vector2 childDesired = child.Measure(availableSize);
      desired.X = MathF.Max(desired.X, child.Position.X + childDesired.X);
      desired.Y = MathF.Max(desired.Y, child.Position.Y + childDesired.Y);
    }
    DesiredSize = ExplicitSize ?? desired;
    return DesiredSize;
  }

  public override void Arrange(Rectangle finalRect)
  {
    Bounds = ResolveOwnBounds(finalRect);

    foreach (UIElement child in Children.OfType<UIElement>())
    {
      child.Arrange(Bounds);
    }
  }
}
