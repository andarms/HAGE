using Hmz.Core._2D;

namespace Hmz.Core.UI;

// Centers each child on its own DesiredSize regardless of the child's own
// HorizontalAlignment/VerticalAlignment, by handing it a final rect already
// sized/positioned to match — the child's own alignment resolves to a no-op
// against a rect that's already exactly its size.
public class CenterContainer : Container
{
  public override void Arrange(Rectangle finalRect)
  {
    Bounds = ResolveOwnBounds(finalRect);

    foreach (UIElement child in Children.OfType<UIElement>())
    {
      float x = Bounds.X + (Bounds.Width - child.DesiredSize.X) / 2f;
      float y = Bounds.Y + (Bounds.Height - child.DesiredSize.Y) / 2f;
      child.Arrange(new Rectangle(x, y, child.DesiredSize.X, child.DesiredSize.Y));
    }
  }
}
