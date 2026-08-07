using Hmz.Core._2D;
using System.Numerics;

namespace Hmz.Core.UI;

public class HBoxContainer : Container
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
      width += childDesired.X;
      height = MathF.Max(height, childDesired.Y);
      count++;
    }
    if (count > 1)
    {
      width += Spacing * (count - 1);
    }
    DesiredSize = ExplicitSize ?? new Vector2(width, height);
    return DesiredSize;
  }

  public override void Arrange(Rectangle finalRect)
  {
    Bounds = ResolveOwnBounds(finalRect);

    List<UIElement> children = Children.OfType<UIElement>().ToList();
    float contentWidth = 0f;
    float totalStretch = 0f;
    foreach (UIElement child in children)
    {
      contentWidth += child.DesiredSize.X;
      totalStretch += child.StretchRatio;
    }
    if (children.Count > 1)
    {
      contentWidth += Spacing * (children.Count - 1);
    }

    float extra = MathF.Max(0f, Bounds.Width - contentWidth);

    float cursorX = 0f;
    if (totalStretch <= 0f)
    {
      cursorX = MainAxisAlignment switch
      {
        UIAlignment.Center => extra / 2f,
        UIAlignment.End => extra,
        _ => 0f,
      };
    }

    foreach (UIElement child in children)
    {
      float extraShare = totalStretch > 0f ? extra * (child.StretchRatio / totalStretch) : 0f;
      float columnWidth = child.DesiredSize.X + extraShare;
      Rectangle column = new(Bounds.X + cursorX, Bounds.Y, columnWidth, Bounds.Height);
      child.Arrange(column);
      cursorX += columnWidth + Spacing;
    }
  }
}
