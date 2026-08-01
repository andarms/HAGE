using Hmz.Core._2D;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Content;
using Hmz.Core.Renderer.Styles;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Hmz.Core.Renderer.OpenGL;

public sealed class OpenGLGraphics : IGraphics
{
  readonly Shader shader;
  readonly uint vao, vbo;
  readonly uint cubeVao, cubeVbo, cubeEbo;
  readonly uint sphereVao, sphereVbo, sphereTriangleEbo, sphereLineEbo;
  readonly uint dynamicVao, dynamicVbo;
  readonly FontRenderer fontRenderer;

  Matrix4x4 projection;

  public OpenGLGraphics(int width, int height)
  {
    string vert = EmbeddedResources.ReadText("Hmz.Core.Resources.Shaders.default.vert");
    string frag = EmbeddedResources.ReadText("Hmz.Core.Resources.Shaders.default.frag");
    shader = new Shader(vert, frag);

    fontRenderer = new FontRenderer(shader);

    float[] verts =
    [
      0, 0, 0, 0, 0,
      1, 0, 0, 1, 0,
      1, 1, 0, 1, 1,
      0, 0, 0, 0, 0,
      1, 1, 0, 1, 1,
      0, 1, 0, 0, 1,
    ];

    vao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(vao);

    vbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, verts, BufferUsageARB.StaticDraw);

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
      Engine.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
    }
    Engine.GL.EnableVertexAttribArray(0);
    Engine.GL.EnableVertexAttribArray(1);

    cubeVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(cubeVao);

    cubeVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, cubeVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, new Cube().GetVertices(), BufferUsageARB.StaticDraw);

    cubeEbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, cubeEbo);
    Engine.GL.BufferData(BufferTargetARB.ElementArrayBuffer, new Cube().GetIndices(), BufferUsageARB.StaticDraw);

    ConfigurePositionOnlyLayout();

    sphereVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(sphereVao);

    sphereVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, sphereVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, new Sphere().GetVertices(), BufferUsageARB.StaticDraw);

    sphereTriangleEbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereTriangleEbo);
    Engine.GL.BufferData(BufferTargetARB.ElementArrayBuffer, new Sphere().GetTriangleIndices(), BufferUsageARB.StaticDraw);

    sphereLineEbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereLineEbo);
    Engine.GL.BufferData(BufferTargetARB.ElementArrayBuffer, new Sphere().GetLineIndices(), BufferUsageARB.StaticDraw);

    ConfigurePositionOnlyLayout();

    // Position-only, re-buffered per draw call — backs the procedural 2D shapes (rectangles, circles, lines).
    dynamicVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(dynamicVao);

    dynamicVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, dynamicVbo);

    ConfigurePositionOnlyLayout();

    Resize(width, height);
  }

  // Binds a plain vec3-position layout to attribute 0, for the VAO currently bound.
  static unsafe void ConfigurePositionOnlyLayout()
  {
    Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
    Engine.GL.EnableVertexAttribArray(0);
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
    Engine.GL.DeleteVertexArray(sphereVao);
    Engine.GL.DeleteBuffer(sphereVbo);
    Engine.GL.DeleteBuffer(sphereTriangleEbo);
    Engine.GL.DeleteBuffer(sphereLineEbo);
    Engine.GL.DeleteVertexArray(dynamicVao);
    Engine.GL.DeleteBuffer(dynamicVbo);
    fontRenderer.Dispose();
    shader.Dispose();
  }

  #region Frame

  public void Clear(Color color)
  {
    Engine.GL.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    Engine.GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
  }

  public void StartFrame() { /* once you batch, the single flush draw call goes here */ }

  public void EndFrame() { }

  #endregion


  #region 2D Drawing

  public void StartMode2D(Camera2D camera)
  {
    shader.Use();
    shader.SetMatrix("uProjection", projection);
    shader.SetMatrix("uView", camera.GetViewMatrix());
    Engine.GL.Disable(EnableCap.DepthTest);
    Engine.GL.BindVertexArray(vao);
  }

  public void EndMode2D() { }

  public void DrawRectangle(float x, float y, float width, float height, RectangleStyle style)
  {
    float radius = Math.Clamp(style.CornerRadius, 0f, MathF.Min(width, height) / 2f);
    Vector2 center = new(x + width / 2f, y + height / 2f);
    Vector2[] ring = ShapeGeometry.BuildRectangleRing(x, y, width, height, radius);
    DrawPolygon(center, ring, style.Fill, style.Border);
  }

  public void DrawRectangle(Rectangle bounds, RectangleStyle style) =>
    DrawRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, style);

  public void DrawLine(float x1, float y1, float x2, float y2, Stroke style) =>
    DrawLines([new Vector2(x1, y1), new Vector2(x2, y2)], style);

  public void DrawLine(Vector2 start, Vector2 end, Stroke style) =>
    DrawLine(start.X, start.Y, end.X, end.Y, style);

  public void DrawLines(Vector2[] points, Stroke style)
  {
    UploadDynamic(ShapeGeometry.ToVertexData(points));
    shader.SetMatrix("uModel", Matrix4x4.Identity);
    shader.SetColor("uColor", style.Color);
    Engine.GL.LineWidth(style.Width);
    Engine.GL.DrawArrays(PrimitiveType.LineStrip, 0, (uint)points.Length);
  }

  public void DrawCircle(float centerX, float centerY, float radius, CircleStyle style)
  {
    Vector2[] ring = ShapeGeometry.BuildCircleRing(centerX, centerY, radius);
    DrawPolygon(new Vector2(centerX, centerY), ring, style.Fill, style.Border);
  }

  public void DrawCircle(Vector2 center, float radius, CircleStyle style) =>
    DrawCircle(center.X, center.Y, radius, style);

  public void DrawTexture(Texture2D texture, float x, float y, float w, float h)
  {
    Engine.GL.Disable(EnableCap.DepthTest);
    Engine.GL.Enable(EnableCap.Blend);
    Engine.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    shader.Use();
    shader.SetMatrix("uProjection", projection);
    shader.SetMatrix("uView", Matrix4x4.Identity);
    Engine.GL.BindVertexArray(vao);
    Engine.GL.ActiveTexture(TextureUnit.Texture0);
    Engine.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
    shader.SetInt("uTexture", 0);
    shader.SetBool("uTextured", true);
    Matrix4x4 model = Matrix4x4.CreateScale(w, h, 1f) * Matrix4x4.CreateTranslation(x, y, 0f);
    shader.SetMatrix("uModel", model);
    shader.SetColor("uColor", Color.White);
    Engine.GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    shader.SetBool("uTextured", false);
    Engine.GL.Disable(EnableCap.Blend);
  }

  // Renders a filled triangle fan and/or a line-loop border around the same point ring —
  // the shared path behind both rounded rectangles and circles.
  void DrawPolygon(Vector2 center, Vector2[] ring, Color? fill, Stroke? border)
  {
    Engine.GL.BindVertexArray(dynamicVao);

    if (fill != null)
    {
      UploadDynamic(ShapeGeometry.ToFanVertexData(center, ring));
      shader.SetMatrix("uModel", Matrix4x4.Identity);
      shader.SetColor("uColor", fill);
      Engine.GL.DrawArrays(PrimitiveType.TriangleFan, 0, (uint)(ring.Length + 2));
    }

    if (border != null)
    {
      UploadDynamic(ShapeGeometry.ToVertexData(ring));
      shader.SetMatrix("uModel", Matrix4x4.Identity);
      shader.SetColor("uColor", border.Color);
      Engine.GL.LineWidth(border.Width);
      Engine.GL.DrawArrays(PrimitiveType.LineLoop, 0, (uint)ring.Length);
    }
  }

  void UploadDynamic(float[] vertices)
  {
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, dynamicVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
  }

  #endregion


  #region 3D Drawing

  public void StartMode3D(Camera3D camera)
  {
    shader.Use();
    shader.SetMatrix("uView", camera.GetViewMatrix());
    shader.SetMatrix("uProjection", camera.GetProjectionMatrix());
    Engine.GL.Enable(EnableCap.DepthTest);
    Engine.GL.BindVertexArray(vao);
  }

  public void EndMode3D() => Engine.GL.Disable(EnableCap.DepthTest);

  public void DrawCube(Cube cube, CubeStyle style)
  {
    Engine.GL.BindVertexArray(cubeVao);
    shader.SetMatrix("uModel", cube.GetRenderMatrix());

    if (style.Wireframe)
    {
      shader.SetColor("uColor", style.Color);
      Engine.GL.LineWidth(style.Width);
      unsafe
      {
        Engine.GL.DrawElements(PrimitiveType.LineLoop, Cube.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
      }
      return;
    }

    shader.SetColor("uColor", style.Color);
    unsafe
    {
      Engine.GL.DrawElements(PrimitiveType.Triangles, Cube.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    if (style.Border != null)
    {
      shader.SetColor("uColor", style.Border.Color);
      Engine.GL.LineWidth(style.Border.Width);
      unsafe
      {
        Engine.GL.DrawElements(PrimitiveType.LineLoop, Cube.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
      }
    }
  }

  public void DrawDebugGrid(int xRows, int zColumns, float cellSize)
  {
    float halfWidth = xRows * cellSize / 2f;
    float halfDepth = zColumns * cellSize / 2f;

    float[] vertices = new float[(xRows + 1 + zColumns + 1) * 2 * 3];
    int offset = 0;

    for (int i = 0; i <= xRows; i++)
    {
      float x = -halfWidth + i * cellSize;
      offset = WriteVertex(vertices, offset, x, 0f, -halfDepth);
      offset = WriteVertex(vertices, offset, x, 0f, halfDepth);
    }

    for (int j = 0; j <= zColumns; j++)
    {
      float z = -halfDepth + j * cellSize;
      offset = WriteVertex(vertices, offset, -halfWidth, 0f, z);
      offset = WriteVertex(vertices, offset, halfWidth, 0f, z);
    }

    Engine.GL.BindVertexArray(dynamicVao);
    UploadDynamic(vertices);
    shader.SetMatrix("uModel", Matrix4x4.Identity);
    shader.SetColor("uColor", Color.Gray);
    Engine.GL.LineWidth(1f);
    Engine.GL.DrawArrays(PrimitiveType.Lines, 0, (uint)(vertices.Length / 3));
  }

  static int WriteVertex(float[] vertices, int offset, float x, float y, float z)
  {
    vertices[offset] = x;
    vertices[offset + 1] = y;
    vertices[offset + 2] = z;
    return offset + 3;
  }

  public void DrawSphere(Sphere sphere, SphereStyle style)
  {
    Engine.GL.BindVertexArray(sphereVao);
    shader.SetMatrix("uModel", sphere.GetRenderMatrix());

    if (style.Wireframe)
    {
      Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereLineEbo);
      shader.SetColor("uColor", style.Color);
      Engine.GL.LineWidth(style.Width);
      unsafe
      {
        Engine.GL.DrawElements(PrimitiveType.Lines, Sphere.LineIndexCount, DrawElementsType.UnsignedInt, (void*)0);
      }
      return;
    }

    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereTriangleEbo);
    shader.SetColor("uColor", style.Color);
    unsafe
    {
      Engine.GL.DrawElements(PrimitiveType.Triangles, Sphere.TriangleIndexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    if (style.Border != null)
    {
      Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereLineEbo);
      shader.SetColor("uColor", style.Border.Color);
      Engine.GL.LineWidth(style.Border.Width);
      unsafe
      {
        Engine.GL.DrawElements(PrimitiveType.Lines, Sphere.LineIndexCount, DrawElementsType.UnsignedInt, (void*)0);
      }
    }
  }

  public void DrawModel(Model model, Matrix4x4 worldMatrix, Matrix4x4[] boneMatrices)
  {
    foreach (Mesh mesh in model.Meshes)
    {
      Engine.GL.BindVertexArray(mesh.Vao);
      shader.SetMatrix("uModel", mesh.NodeTransform * worldMatrix);
      shader.SetColor("uColor", Color.White);

      bool skinned = mesh.IsSkinned && boneMatrices.Length > 0;
      shader.SetBool("uSkinned", skinned);
      if (skinned)
      {
        shader.SetMatrixArray("uBones", boneMatrices);
      }

      if (mesh.Texture != null)
      {
        Engine.GL.ActiveTexture(TextureUnit.Texture0);
        Engine.GL.BindTexture(TextureTarget.Texture2D, mesh.Texture.Handle);
        shader.SetInt("uTexture", 0);
        shader.SetBool("uTextured", true);
      }

      unsafe
      {
        Engine.GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
      }

      shader.SetBool("uTextured", false);
      if (skinned)
      {
        shader.SetBool("uSkinned", false);
      }
    }
  }

  #endregion


  #region Text Drawing

  public void DrawText(string text, float x, float y, TextStyle style) =>
    fontRenderer.DrawText(text, x, y, style, projection);

  #endregion
}
