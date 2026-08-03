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

    // Row-vector convention (v' = v * M): move to camera-space origin first, then
    // un-rotate, then zoom, then offset into the target's screen position — scaling/
    // rotating before the -Position translation would pivot around the world origin
    // instead of the camera, making pan and zoom interact incorrectly.
    return translation * rotation * scale * targetTranslation;
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
    Zoom = Math.Clamp(Zoom + amount, 0.1f, 10f);
  }

  public void ZoomOut(float amount)
  {
    Zoom = Math.Clamp(Zoom - amount, 0.1f, 10f);
  }
}
