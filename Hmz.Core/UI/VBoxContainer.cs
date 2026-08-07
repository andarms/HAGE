using Hmz.Core._2D;
using System.Numerics;

namespace Hmz.Core.UI;

public class VBoxContainer : Container
{
  public float Spacing { get; set; } = 4f;
  public UIAlignment MainAxisAlignment { get; set; } = UIAlignment.Start;

  public override Vector2 Measure(Vector2 availableSize)
  {
    float width = 0f;
    float height = 0f;
    int count = 0;
    foreach (UIElement child in Children.OfType<UIElement>())
    {
      Vector2 childDesired = child.Measure(new Vector2(availableSize.X, availableSize.Y));
      width = MathF.Max(width, childDesired.X);
      height += childDesired.Y;
      count++;
    }
    if (count > 1)
    {
      height += Spacing * (count - 1);
    }
    DesiredSize = ExplicitSize ?? new Vector2(width, height);
    return DesiredSize;
  }

  public override void Arrange(Rectangle finalRect)
  {
    Bounds = ResolveOwnBounds(finalRect);

    List<UIElement> children = Children.OfType<UIElement>().ToList();
    float contentHeight = 0f;
    float totalStretch = 0f;
    foreach (UIElement child in children)
    {
      contentHeight += child.DesiredSize.Y;
      totalStretch += child.StretchRatio;
    }
    if (children.Count > 1)
    {
      contentHeight += Spacing * (children.Count - 1);
    }

    float extra = MathF.Max(0f, Bounds.Height - contentHeight);

    float cursorY = 0f;
    if (totalStretch <= 0f)
    {
      cursorY = MainAxisAlignment switch
      {
        UIAlignment.Center => extra / 2f,
        UIAlignment.End => extra,
        _ => 0f,
      };
    }

    foreach (UIElement child in children)
    {
      float extraShare = totalStretch > 0f ? extra * (child.StretchRatio / totalStretch) : 0f;
      float rowHeight = child.DesiredSize.Y + extraShare;
      Rectangle row = new(Bounds.X, Bounds.Y + cursorY, Bounds.Width, rowHeight);
      child.Arrange(row);
      cursorY += rowHeight + Spacing;
    }
  }
}
