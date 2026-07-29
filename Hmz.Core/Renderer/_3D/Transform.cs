namespace Hmz.Core.Renderer._3D;

using System.Numerics;

public class Transform
{
  public Vector3 Position { get; set; } = Vector3.Zero;
  public Vector3 Rotation { get; set; } = Vector3.Zero;
  public Vector3 Scale { get; set; } = Vector3.One;

  public Matrix4x4 GetModelMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Position);
    Matrix4x4 rotationX = Matrix4x4.CreateRotationX(Rotation.X);
    Matrix4x4 rotationY = Matrix4x4.CreateRotationY(Rotation.Y);
    Matrix4x4 rotationZ = Matrix4x4.CreateRotationZ(Rotation.Z);
    Matrix4x4 scale = Matrix4x4.CreateScale(Scale);

    return scale * rotationZ * rotationY * rotationX * translation;
  }
}
