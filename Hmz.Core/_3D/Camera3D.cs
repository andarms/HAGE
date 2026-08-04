using System.Numerics;

namespace Hmz.Core._3D;

public class Camera3D
{
  public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
  public Vector3 Target { get; set; } = Vector3.Zero;
  public Vector3 Up { get; set; } = Vector3.UnitY;

  public float FieldOfView { get; set; } = MathF.PI / 4f;
  public float AspectRatio { get; set; } = 16f / 9f;
  public float NearPlane { get; set; } = 0.1f;
  public float FarPlane { get; set; } = 100f;

  public Matrix4x4 GetViewMatrix() => Matrix4x4.CreateLookAt(Position, Target, Up);

  public Matrix4x4 GetProjectionMatrix() => Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);

  public Matrix4x4 GetViewProjectionMatrix() => GetViewMatrix() * GetProjectionMatrix();

  public void Move(Vector3 direction, float amount)
  {
    Position += Vector3.Normalize(direction) * amount;
    Target += Vector3.Normalize(direction) * amount;
  }

  public void Follow(Vector3 targetPosition, Vector3 offset, float speed, float dt)
  {
    float t = MathF.Min(speed * dt, 1f);
    Position = Vector3.Lerp(Position, targetPosition + offset, t);
    Target = Vector3.Lerp(Target, targetPosition, t);
  }

  public void Rotate(float yaw, float pitch)
  {
    Vector3 direction = Target - Position;
    direction = Vector3.Transform(direction, Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0));
    Target = Position + direction;
  }

  public void Orbit(Vector3 point, float yaw, float pitch)
  {
    Vector3 direction = Position - point;
    direction = Vector3.Transform(direction, Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0));
    Position = point + direction;
    Target = point;
  }
}
