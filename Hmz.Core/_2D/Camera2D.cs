using System.Numerics;

namespace Hmz.Core._2D;

public class Camera2D
{
  public Vector2 Position { get; set; } = Vector2.Zero;
  public Vector2 Target { get; set; } = Vector2.Zero;
  public float Rotation { get; set; } = 0f;
  public float Zoom { get; set; } = 1f;

  public Matrix4x4 GetViewMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0f);
    Matrix4x4 rotation = Matrix4x4.CreateRotationZ(-Rotation);
    Matrix4x4 scale = Matrix4x4.CreateScale(Zoom, Zoom, 1f);
    Matrix4x4 targetTranslation = Matrix4x4.CreateTranslation(Target.X, Target.Y, 0f);

    return scale * rotation * translation * targetTranslation;
  }

  public void Move(Vector2 direction, float amount)
  {
    Position += Vector2.Normalize(direction) * amount;
    Target += Vector2.Normalize(direction) * amount;
  }

  public void Rotate(float angle)
  {
    Rotation += angle;
  }

  public void ZoomIn(float amount)
  {
    Zoom += amount;
    if (Zoom > 10f) { Zoom = 10f; } // Prevent zooming in too much
  }

  public void ZoomOut(float amount)
  {
    Zoom -= amount;
    if (Zoom < 0.1f) { Zoom = 0.1f; } // Prevent zooming out too much
  }
}
