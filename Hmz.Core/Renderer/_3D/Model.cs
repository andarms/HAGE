namespace Hmz.Core.Renderer._3D;

using System.Numerics;
using Hmz.Core;

public class Model : IDisposable
{
  public string Path { get; init; } = "";
  public Transform Transform { get; init; } = new Transform();
  public List<Mesh> Meshes { get; init; } = [];

  public Matrix4x4 GetModelMatrix()
  {
    Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.Position);
    Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y, Transform.Rotation.X, Transform.Rotation.Z);
    Matrix4x4 scale = Matrix4x4.CreateScale(Transform.Scale);
    return scale * rotation * translation;
  }

  public void Dispose()
  {
    foreach (Mesh mesh in Meshes)
    {
      Engine.GL.DeleteVertexArray(mesh.Vao);
      Engine.GL.DeleteBuffer(mesh.Vbo);
      Engine.GL.DeleteBuffer(mesh.Ebo);
    }
  }
}