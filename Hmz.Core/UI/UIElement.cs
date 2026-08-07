using Hmz.Core._2D;
using Hmz.Core.GOM;
using System.Numerics;

namespace Hmz.Core.UI;

public enum UIAlignment
{
  Start,
  Center,
  End,
  Stretch,
}

public class UIElement : GameObject
{
  // IGraphics.DrawRectangle is batched and only reaches the GPU at EndMode2D, while
  // DrawText is immediate. A widget that draws a rectangle then text in the same
  // OnDraw would otherwise have its text rendered first and its rectangle composited
  // on top of it later, at the shared end-of-frame flush. Forcing a flush here (cheap:
  // StartMode2D/EndMode2D don't reset the pending batch, they just drain it) between
  // the two keeps a widget's own background behind its own text.
  public static readonly Camera2D CanvasCamera = new();

  protected static void FlushBackground()
  {
    Engine.Graphics.EndMode2D();
    Engine.Graphics.StartMode2D(CanvasCamera);
  }

  public Vector2 Position { get; set; } = Vector2.Zero;
  public Vector2? ExplicitSize { get; set; }
  public bool Visible { get; set; } = true;
  public bool BlocksInput { get; set; }

  public UIAlignment HorizontalAlignment { get; set; } = UIAlignment.Start;
  public UIAlignment VerticalAlignment { get; set; } = UIAlignment.Start;
  public float StretchRatio { get; set; } = 0f;

  public void FillParent()
  {
    HorizontalAlignment = UIAlignment.Stretch;
    VerticalAlignment = UIAlignment.Stretch;
  }

  public Rectangle Bounds { get; protected set; } = new(0f, 0f, 0f, 0f);
  public Vector2 DesiredSize { get; protected set; } = Vector2.Zero;

  public virtual Vector2 Measure(Vector2 availableSize)
  {
    DesiredSize = ExplicitSize ?? Vector2.Zero;
    return DesiredSize;
  }

  public virtual void Arrange(Rectangle finalRect)
  {
    Bounds = ResolveOwnBounds(finalRect);
  }

  // Shared by every container's Arrange override for sizing/positioning itself
  // within the rect its own parent handed it, before it lays out its children.
  protected Rectangle ResolveOwnBounds(Rectangle finalRect)
  {
    float width = ExplicitSize?.X ?? (HorizontalAlignment == UIAlignment.Stretch ? finalRect.Width : DesiredSize.X);
    float height = ExplicitSize?.Y ?? (VerticalAlignment == UIAlignment.Stretch ? finalRect.Height : DesiredSize.Y);

    float x = HorizontalAlignment switch
    {
      UIAlignment.Center => finalRect.X + (finalRect.Width - width) / 2f,
      UIAlignment.End => finalRect.X + finalRect.Width - width,
      _ => finalRect.X,
    };
    float y = VerticalAlignment switch
    {
      UIAlignment.Center => finalRect.Y + (finalRect.Height - height) / 2f,
      UIAlignment.End => finalRect.Y + finalRect.Height - height,
      _ => finalRect.Y,
    };

    return new Rectangle(x + Position.X, y + Position.Y, width, height);
  }

  public sealed override void HandleInput()
  {
    if (!Visible) return;

    if (BlocksInput && Bounds.Contains(Engine.Input.MousePosition.X, Engine.Input.MousePosition.Y))
    {
      Engine.Input.CaptureMouse();
    }

    OnHandleInput();
    Components.HandleInput();
    foreach (GameObject child in Children)
    {
      child.HandleInput();
    }
  }

  protected virtual void OnHandleInput() { }

  public sealed override void Draw()
  {
    if (!Visible) return;

    OnDraw();
    base.Draw();
  }

  protected virtual void OnDraw() { }

  // GameObject.Debug() draws 3D-only primitives (DrawSphere/DrawCube) meant for
  // StartMode3D. The canvas layer is drawn inside StartMode2D, so the default 3D
  // debug visual would be both meaningless and issued in the wrong render pass.
  public sealed override void Debug() { }
}
