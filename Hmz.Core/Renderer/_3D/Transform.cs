namespace Hmz.Core.Renderer._3D;

using System.Numerics;

public class Transform
{
  public Vector3 Position { get; set; } = Vector3.Zero;
  public Vector3 Rotation { get; set; } = Vector3.Zero;
  public Vector3 Scale { get; set; } = Vector3.One;

  public static Transform FromMatrix(Matrix4x4 matrix)
  {
    if (!Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 position))
    {
      throw new InvalidOperationException("The matrix cannot be decomposed into a transform.");
    }

    float pitch = MathF.Asin(Math.Clamp(2f * (rotation.W * rotation.X - rotation.Z * rotation.Y), -1f, 1f));
    float yaw = MathF.Atan2(
      2f * (rotation.W * rotation.Y + rotation.Z * rotation.X),
      1f - 2f * (rotation.X * rotation.X + rotation.Y * rotation.Y));
    float roll = MathF.Atan2(
      2f * (rotation.W * rotation.Z + rotation.X * rotation.Y),
      1f - 2f * (rotation.X * rotation.X + rotation.Z * rotation.Z));

    return new Transform
    {
      Position = position,
      Rotation = new Vector3(pitch, yaw, roll),
      Scale = scale,
    };
  }

  public Matrix4x4 GetLocalMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Position);
    Matrix4x4 rotationX = Matrix4x4.CreateRotationX(Rotation.X);
    Matrix4x4 rotationY = Matrix4x4.CreateRotationY(Rotation.Y);
    Matrix4x4 rotationZ = Matrix4x4.CreateRotationZ(Rotation.Z);
    Matrix4x4 scale = Matrix4x4.CreateScale(Scale);

    return scale * rotationZ * rotationY * rotationX * translation;
  }
}
