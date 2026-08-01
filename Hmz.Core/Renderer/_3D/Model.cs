namespace Hmz.Core.Renderer._3D;

using System.Numerics;
using Hmz.Core;

public class Model : IDisposable
{
  public string Path { get; init; } = "";
  public List<Mesh> Meshes { get; init; } = [];

  public SkeletonNode? Skeleton { get; init; }
  public Dictionary<string, int> BoneNameToIndex { get; init; } = [];
  public Matrix4x4[] BoneOffsets { get; init; } = [];
  public Dictionary<string, AnimationClip> Animations { get; init; } = [];

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
