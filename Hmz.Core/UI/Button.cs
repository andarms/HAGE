using Hmz.Core.Input;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using System.Numerics;

namespace Hmz.Core.UI;

public class Button : UIElement
{
  public string Text { get; set; } = string.Empty;
  public TextStyle TextStyle { get; set; } = new();
  public RectangleStyle NormalStyle { get; set; } = new() { Fill = Color.Gray };
  public RectangleStyle HoverStyle { get; set; } = new() { Fill = Color.LightGray };
  public RectangleStyle PressedStyle { get; set; } = new() { Fill = Color.DarkGray };
  public Vector2 Padding { get; set; } = new(12f, 6f);

  public event Action? Clicked;

  bool hovered;
  bool pressed;

  public Button()
  {
    BlocksInput = true;
  }

  public override Vector2 Measure(Vector2 availableSize)
  {
    Vector2 textSize = Engine.Graphics.MeasureText(Text, TextStyle);
    DesiredSize = ExplicitSize ?? textSize + Padding * 2f;
    return DesiredSize;
  }

  protected override void OnHandleInput()
  {
    hovered = Bounds.Contains(Engine.Input.MousePosition.X, Engine.Input.MousePosition.Y);

    if (hovered && Engine.Input.IsMouseButtonJustPressed(MouseButton.Left))
    {
      pressed = true;
    }

    if (pressed && Engine.Input.IsMouseButtonJustReleased(MouseButton.Left))
    {
      pressed = false;
      if (hovered)
      {
        Clicked?.Invoke();
      }
    }
  }

  protected override void DrawSelf()
  {
    RectangleStyle style = pressed ? PressedStyle : hovered ? HoverStyle : NormalStyle;
    Engine.Graphics.DrawRectangle(Bounds, style);
    FlushBackground();

    Vector2 textSize = Engine.Graphics.MeasureText(Text, TextStyle);
    float textX = Bounds.X + (Bounds.Width - textSize.X) / 2f;
    float textY = Bounds.Y + (Bounds.Height - textSize.Y) / 2f;
    Engine.Graphics.DrawText(Text, textX, textY, TextStyle);
  }
}
