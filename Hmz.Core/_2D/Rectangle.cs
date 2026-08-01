namespace Hmz.Core._2D;

public class Rectangle(float x, float y, float width, float height)
{
  public float X { get; set; } = x;
  public float Y { get; set; } = y;
  public float Width { get; set; } = width;
  public float Height { get; set; } = height;

  public float Left => X;
  public float Right => X + Width;
  public float Top => Y;
  public float Bottom => Y + Height;

  public bool Contains(float px, float py) => px >= Left && px <= Right && py >= Top && py <= Bottom;

  public bool Intersects(Rectangle other) => Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
}
