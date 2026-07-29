namespace Hmz.Core.Renderer._2D;

public class Circle(float x, float y, float radius)
{
  public float X { get; set; } = x;
  public float Y { get; set; } = y;
  public float Radius { get; set; } = radius;

  public bool Contains(float px, float py)
  {
    float dx = px - X;
    float dy = py - Y;
    return dx * dx + dy * dy <= Radius * Radius;
  }

  public bool Intersects(Circle other)
  {
    float dx = other.X - X;
    float dy = other.Y - Y;
    float distanceSquared = dx * dx + dy * dy;
    float radiusSum = Radius + other.Radius;
    return distanceSquared <= radiusSum * radiusSum;
  }

  public bool Intersects(Rectangle rect)
  {
    float closestX = Math.Clamp(X, rect.Left, rect.Right);
    float closestY = Math.Clamp(Y, rect.Top, rect.Bottom);

    float dx = X - closestX;
    float dy = Y - closestY;

    return dx * dx + dy * dy <= Radius * Radius;
  }
}