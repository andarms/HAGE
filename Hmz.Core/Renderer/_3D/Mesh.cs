namespace Hmz.Core.Renderer._3D;

using System.Numerics;
using Hmz.Core.Renderer._2D;

public class Mesh
{
  public uint Vao { get; init; }
  public uint Vbo { get; init; }
  public uint Ebo { get; init; }
  public uint IndexCount { get; init; }
  public Matrix4x4 NodeTransform { get; init; } = Matrix4x4.Identity;
  public Texture2D? Texture { get; init; }
}
