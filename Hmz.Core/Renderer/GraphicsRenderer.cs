using Hmz.Core.Content;
using Hmz.Core.Graphics._2D;
using Hmz.Core.Graphics._3D;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Hmz.Core.Graphics;

public sealed class GraphicsRenderer : IDisposable
{

  readonly Shader shader;
  readonly uint vao, vbo;
  readonly uint cubeVao, cubeVbo, cubeEbo;

  Matrix4x4 projection;

  public GraphicsRenderer(int width, int height)
  {
    string vert = EmbeddedResources.ReadText("Hmz.Core.Resources.Shaders.default.vert");
    string frag = EmbeddedResources.ReadText("Hmz.Core.Resources.Shaders.default.frag");
    shader = new Shader(vert, frag);

    float[] verts = [0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1];

    vao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(vao);

    vbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, verts, BufferUsageARB.StaticDraw);

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), (void*)0);
    }
    Engine.GL.EnableVertexAttribArray(0);

    cubeVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(cubeVao);

    cubeVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, cubeVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, new Cube().GetVertices(), BufferUsageARB.StaticDraw);

    cubeEbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, cubeEbo);
    Engine.GL.BufferData(BufferTargetARB.ElementArrayBuffer, new Cube().GetIndices(), BufferUsageARB.StaticDraw);

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
    }
    Engine.GL.EnableVertexAttribArray(0);

    Resize(width, height);
  }

  // origin top-left, y down
  public void Resize(int w, int h) => projection = Matrix4x4.CreateOrthographicOffCenter(0, w, h, 0, -1f, 1f);


  public void Dispose()
  {
    Engine.GL.DeleteVertexArray(vao);
    Engine.GL.DeleteBuffer(vbo);
    Engine.GL.DeleteVertexArray(cubeVao);
    Engine.GL.DeleteBuffer(cubeVbo);
    Engine.GL.DeleteBuffer(cubeEbo);
    shader.Dispose();
  }


  #region 2D


  public void BeginFrame(Color clear)
  {
    Engine.GL.ClearColor(clear.R / 255f, clear.G / 255f, clear.B / 255f, clear.A / 255f);
    Engine.GL.Clear((uint)ClearBufferMask.ColorBufferBit);
    shader.Use();
    shader.SetMatrix("uProjection", projection);
    shader.SetMatrix("uView", Matrix4x4.Identity);
    Engine.GL.BindVertexArray(vao);
  }

  public void End() { /* once you batch, the sinEngine.GLe flush draw call goes here */ }


  public void DrawRectangle(float x, float y, float w, float h, Color color)
  {
    Matrix4x4 model = Matrix4x4.CreateScale(w, h, 1f) * Matrix4x4.CreateTranslation(x, y, 0f);
    shader.SetMatrix("uModel", model);
    shader.SetColor("uColor", color);
    Engine.GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
  }

  public void DrawRectangle(Rectangle bounds, Color color) => DrawRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, color);


  #endregion


  #region 3D
  public void BeginFrame(Camera3D camera)
  {
    Engine.GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    Engine.GL.Enable(EnableCap.DepthTest);

    Matrix4x4 view = camera.GetViewMatrix();
    Matrix4x4 projection = camera.GetProjectionMatrix();

    shader.Use();
    shader.SetMatrix("uView", view);
    shader.SetMatrix("uProjection", projection);
    Engine.GL.BindVertexArray(vao);
  }

  public void DrawCube(Cube cube, Color color)
  {
    Engine.GL.BindVertexArray(cubeVao);
    shader.SetMatrix("uModel", cube.GetModelMatrix());
    shader.SetColor("uColor", color);
    unsafe
    {
      Engine.GL.DrawElements(PrimitiveType.Triangles, Cube.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
  }

  public void DrawCubeWires(Cube cube, Color color)
  {
    Engine.GL.BindVertexArray(cubeVao);
    shader.SetMatrix("uModel", cube.GetModelMatrix());
    shader.SetColor("uColor", color);

    Engine.GL.LineWidth(1f);

    unsafe
    {
      Engine.GL.DrawElements(PrimitiveType.LineLoop, Cube.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
  }

  public void EndFrame()
  {
    Engine.GL.Disable(EnableCap.DepthTest);
  }
  #endregion
}
