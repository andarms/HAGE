namespace Hmz.Core.Graphics._3D;

using System.Numerics;

public class Model
{
  public string Path { get; init; } = "";
  public Transform Transform { get; init; } = new Transform();

  public Matrix4x4 GetModelMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y, Transform.Rotation.X, Transform.Rotation.Z);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    return scale * rotation * translation;
  }
}