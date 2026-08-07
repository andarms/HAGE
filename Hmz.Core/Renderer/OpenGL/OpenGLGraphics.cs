using Hmz.Core._2D;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Content;
using Hmz.Core.Renderer.Styles;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Hmz.Core.Renderer.OpenGL;

public sealed class OpenGLGraphics : IGraphics
{
  readonly Shader shader;
  readonly uint vao, vbo;
  readonly uint cubeVao, cubeVbo, cubeEbo, cubeInstanceVbo;
  readonly uint sphereVao, sphereVbo, sphereTriangleEbo, sphereLineEbo, sphereInstanceVbo;
  readonly uint batch2DVao, batch2DVbo;
  readonly uint gridVao, gridVbo;
  readonly FontRenderer fontRenderer;

  int drawCallCount;
  public int DrawCallCount => drawCallCount + fontRenderer.DrawCallCount;

  // 2D fills and strokes queued between StartMode2D/EndMode2D, flushed as a batch.
  readonly List<float> pendingFillVertices = [];
  readonly Dictionary<float, List<float>> pendingLinesByWidth = [];

  const int Batch2DFloatsPerVertex = 7; // pos(3) + color(4)

  // Per-instance transform + color for GPU-instanced Cube/Sphere draws.
  [StructLayout(LayoutKind.Sequential)]
  readonly record struct InstanceData(Matrix4x4 Model, Vector4 Color);

  readonly List<InstanceData> pendingCubeFill = [];
  readonly Dictionary<float, List<InstanceData>> pendingCubeWireframe = [];
  readonly Dictionary<float, List<InstanceData>> pendingCubeBorder = [];

  readonly List<InstanceData> pendingSphereFill = [];
  readonly Dictionary<float, List<InstanceData>> pendingSphereWireframe = [];
  readonly Dictionary<float, List<InstanceData>> pendingSphereBorder = [];

  // Non-skinned mesh draws queued between StartMode3D/EndMode3D, grouped by shared Mesh.
  readonly Dictionary<Mesh, List<Matrix4x4>> pendingMeshInstances = [];

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

    cubeInstanceVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, cubeInstanceVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, new float[20], BufferUsageARB.DynamicDraw);
    ConfigureInstanceLayout(includeColor: true);

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

    sphereInstanceVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, sphereInstanceVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, new float[20], BufferUsageARB.DynamicDraw);
    ConfigureInstanceLayout(includeColor: true);

    batch2DVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(batch2DVao);

    batch2DVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, batch2DVbo);

    ConfigurePositionColorLayout();

    gridVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(gridVao);

    gridVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, gridVbo);

    ConfigurePositionOnlyLayout();

    Resize(width, height);
  }

  // Binds a plain vec3-position layout to attribute 0, for the VAO currently bound.
  static unsafe void ConfigurePositionOnlyLayout()
  {
    Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
    Engine.GL.EnableVertexAttribArray(0);
  }

  static unsafe void ConfigurePositionColorLayout()
  {
    Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Batch2DFloatsPerVertex * sizeof(float), (void*)0);
    Engine.GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, Batch2DFloatsPerVertex * sizeof(float), (void*)(3 * sizeof(float)));
    Engine.GL.EnableVertexAttribArray(0);
    Engine.GL.EnableVertexAttribArray(8);
  }

  // mat4 at locations 4-7 (one vec4 row each) + optional color at 8, divisor 1 = per-instance.
  static unsafe void ConfigureInstanceLayout(bool includeColor)
  {
    int stride = (includeColor ? 20 : 16) * sizeof(float);

    for (uint row = 0; row < 4; row++)
    {
      Engine.GL.VertexAttribPointer(4 + row, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)(row * 4 * sizeof(float)));
      Engine.GL.EnableVertexAttribArray(4 + row);
      Engine.GL.VertexAttribDivisor(4 + row, 1);
    }

    if (includeColor)
    {
      Engine.GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)(16 * sizeof(float)));
      Engine.GL.EnableVertexAttribArray(8);
      Engine.GL.VertexAttribDivisor(8, 1);
    }
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
    Engine.GL.DeleteBuffer(cubeInstanceVbo);
    Engine.GL.DeleteVertexArray(sphereVao);
    Engine.GL.DeleteBuffer(sphereVbo);
    Engine.GL.DeleteBuffer(sphereTriangleEbo);
    Engine.GL.DeleteBuffer(sphereLineEbo);
    Engine.GL.DeleteBuffer(sphereInstanceVbo);
    Engine.GL.DeleteVertexArray(batch2DVao);
    Engine.GL.DeleteBuffer(batch2DVbo);
    Engine.GL.DeleteVertexArray(gridVao);
    Engine.GL.DeleteBuffer(gridVbo);
    fontRenderer.Dispose();
    shader.Dispose();
  }

  #region Frame

  public void Clear(Color color)
  {
    Engine.GL.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    Engine.GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
  }

  // Batches flush at EndMode2D/EndMode3D, not here — a frame can hold multiple mode blocks.
  public void StartFrame()
  {
    drawCallCount = 0;
    fontRenderer.ResetDrawCallCount();
  }

  public void EndFrame() { }

  void CountDrawCall() => drawCallCount++;

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

  // Fills flush before strokes, so a shape's own border draws over its own fill. Fill/stroke
  // order across different shapes is not preserved.
  public void EndMode2D()
  {
    FlushFills2D();
    FlushStrokes2D();
  }

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
    List<float> group = GetLineGroup(style.Width);
    for (int i = 0; i < points.Length - 1; i++)
    {
      AppendLineVertex(group, points[i], style.Color);
      AppendLineVertex(group, points[i + 1], style.Color);
    }
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
    CountDrawCall();
    shader.SetBool("uTextured", false);
    Engine.GL.Disable(EnableCap.Blend);
  }

  // Queues a filled triangle fan and/or a line-loop border around the same point ring —
  // the shared path behind both rounded rectangles and circles.
  void DrawPolygon(Vector2 center, Vector2[] ring, Color? fill, Stroke? border)
  {
    if (fill != null)
    {
      for (int i = 0; i < ring.Length; i++)
      {
        Vector2 next = ring[(i + 1) % ring.Length];
        AppendFillVertex(center, fill);
        AppendFillVertex(ring[i], fill);
        AppendFillVertex(next, fill);
      }
    }

    if (border != null)
    {
      List<float> group = GetLineGroup(border.Width);
      for (int i = 0; i < ring.Length; i++)
      {
        Vector2 next = ring[(i + 1) % ring.Length];
        AppendLineVertex(group, ring[i], border.Color);
        AppendLineVertex(group, next, border.Color);
      }
    }
  }

  void AppendFillVertex(Vector2 p, Color c)
  {
    pendingFillVertices.Add(p.X);
    pendingFillVertices.Add(p.Y);
    pendingFillVertices.Add(0f);
    pendingFillVertices.Add(c.R / 255f);
    pendingFillVertices.Add(c.G / 255f);
    pendingFillVertices.Add(c.B / 255f);
    pendingFillVertices.Add(c.A / 255f);
  }

  List<float> GetLineGroup(float width)
  {
    if (!pendingLinesByWidth.TryGetValue(width, out List<float>? group))
    {
      group = [];
      pendingLinesByWidth[width] = group;
    }
    return group;
  }

  static void AppendLineVertex(List<float> group, Vector2 p, Color c)
  {
    group.Add(p.X);
    group.Add(p.Y);
    group.Add(0f);
    group.Add(c.R / 255f);
    group.Add(c.G / 255f);
    group.Add(c.B / 255f);
    group.Add(c.A / 255f);
  }

  void FlushFills2D()
  {
    if (pendingFillVertices.Count == 0) return;

    Engine.GL.BindVertexArray(batch2DVao);
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, batch2DVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, CollectionsMarshal.AsSpan(pendingFillVertices), BufferUsageARB.DynamicDraw);

    shader.SetMatrix("uModel", Matrix4x4.Identity);
    shader.SetBool("uUseVertexColor", true);
    Engine.GL.DrawArrays(PrimitiveType.Triangles, 0, (uint)(pendingFillVertices.Count / Batch2DFloatsPerVertex));
    CountDrawCall();
    shader.SetBool("uUseVertexColor", false);

    pendingFillVertices.Clear();
  }

  // LineWidth is GL state, not per-vertex, so strokes are grouped by width.
  void FlushStrokes2D()
  {
    if (pendingLinesByWidth.Count == 0) return;

    Engine.GL.BindVertexArray(batch2DVao);
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, batch2DVbo);
    shader.SetMatrix("uModel", Matrix4x4.Identity);
    shader.SetBool("uUseVertexColor", true);

    foreach ((float width, List<float> vertices) in pendingLinesByWidth)
    {
      Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, CollectionsMarshal.AsSpan(vertices), BufferUsageARB.DynamicDraw);
      Engine.GL.LineWidth(width);
      Engine.GL.DrawArrays(PrimitiveType.Lines, 0, (uint)(vertices.Count / Batch2DFloatsPerVertex));
      CountDrawCall();
    }

    shader.SetBool("uUseVertexColor", false);
    pendingLinesByWidth.Clear();
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

  public void EndMode3D()
  {
    FlushCubes();
    FlushSpheres();
    FlushMeshInstances();
    Engine.GL.Disable(EnableCap.DepthTest);
  }

  public void DrawCube(Cube cube, CubeStyle style) => AddCubeInstance(cube.GetRenderMatrix(), style);

  public void DrawCube(Cube cube, Matrix4x4 worldMatrix, CubeStyle style)
  {
    AddCubeInstance(Matrix4x4.CreateScale(cube.Size) * worldMatrix, style);
  }

  void AddCubeInstance(Matrix4x4 model, CubeStyle style)
  {
    if (style.Wireframe)
    {
      GetInstanceGroup(pendingCubeWireframe, style.Width).Add(new InstanceData(model, ToVector4(style.Color)));
      return;
    }

    pendingCubeFill.Add(new InstanceData(model, ToVector4(style.Color)));

    if (style.Border != null)
    {
      GetInstanceGroup(pendingCubeBorder, style.Border.Width).Add(new InstanceData(model, ToVector4(style.Border.Color)));
    }
  }

  static Vector4 ToVector4(Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

  static List<InstanceData> GetInstanceGroup(Dictionary<float, List<InstanceData>> groups, float key)
  {
    if (!groups.TryGetValue(key, out List<InstanceData>? group))
    {
      group = [];
      groups[key] = group;
    }
    return group;
  }

  void FlushCubes()
  {
    Engine.GL.BindVertexArray(cubeVao);
    DrawCubeInstances(pendingCubeFill, PrimitiveType.Triangles, null);
    pendingCubeFill.Clear();

    foreach ((float width, List<InstanceData> instances) in pendingCubeWireframe)
    {
      DrawCubeInstances(instances, PrimitiveType.LineLoop, width);
    }
    pendingCubeWireframe.Clear();

    foreach ((float width, List<InstanceData> instances) in pendingCubeBorder)
    {
      DrawCubeInstances(instances, PrimitiveType.LineLoop, width);
    }
    pendingCubeBorder.Clear();
  }

  unsafe void DrawCubeInstances(List<InstanceData> instances, PrimitiveType primitiveType, float? lineWidth)
  {
    if (instances.Count == 0) return;

    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, cubeInstanceVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, MemoryMarshal.Cast<InstanceData, float>(CollectionsMarshal.AsSpan(instances)), BufferUsageARB.DynamicDraw);

    shader.SetBool("uInstancedTransform", true);
    shader.SetBool("uUseVertexColor", true);
    if (lineWidth is float width) Engine.GL.LineWidth(width);

    Engine.GL.DrawElementsInstanced(primitiveType, Cube.IndexCount, DrawElementsType.UnsignedInt, (void*)0, (uint)instances.Count);
    CountDrawCall();

    shader.SetBool("uInstancedTransform", false);
    shader.SetBool("uUseVertexColor", false);
  }

  public void DrawDebugGrid(int xRows, int zColumns, float cellSize, float y = 0f, float offsetX = 0f, float offsetZ = 0f)
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

    Engine.GL.BindVertexArray(gridVao);
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, gridVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);
    shader.SetMatrix("uModel", Matrix4x4.CreateTranslation(offsetX, y, offsetZ));
    shader.SetColor("uColor", Color.Gray);
    Engine.GL.LineWidth(1f);
    Engine.GL.DrawArrays(PrimitiveType.Lines, 0, (uint)(vertices.Length / 3));
    CountDrawCall();
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
    Matrix4x4 model = sphere.GetRenderMatrix();

    if (style.Wireframe)
    {
      GetInstanceGroup(pendingSphereWireframe, style.Width).Add(new InstanceData(model, ToVector4(style.Color)));
      return;
    }

    pendingSphereFill.Add(new InstanceData(model, ToVector4(style.Color)));

    if (style.Border != null)
    {
      GetInstanceGroup(pendingSphereBorder, style.Border.Width).Add(new InstanceData(model, ToVector4(style.Border.Color)));
    }
  }

  void FlushSpheres()
  {
    Engine.GL.BindVertexArray(sphereVao);

    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereTriangleEbo);
    DrawSphereInstances(pendingSphereFill, PrimitiveType.Triangles, Sphere.TriangleIndexCount, null);
    pendingSphereFill.Clear();

    Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, sphereLineEbo);
    foreach ((float width, List<InstanceData> instances) in pendingSphereWireframe)
    {
      DrawSphereInstances(instances, PrimitiveType.Lines, Sphere.LineIndexCount, width);
    }
    pendingSphereWireframe.Clear();

    foreach ((float width, List<InstanceData> instances) in pendingSphereBorder)
    {
      DrawSphereInstances(instances, PrimitiveType.Lines, Sphere.LineIndexCount, width);
    }
    pendingSphereBorder.Clear();
  }

  unsafe void DrawSphereInstances(List<InstanceData> instances, PrimitiveType primitiveType, uint indexCount, float? lineWidth)
  {
    if (instances.Count == 0) return;

    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, sphereInstanceVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, MemoryMarshal.Cast<InstanceData, float>(CollectionsMarshal.AsSpan(instances)), BufferUsageARB.DynamicDraw);

    shader.SetBool("uInstancedTransform", true);
    shader.SetBool("uUseVertexColor", true);
    if (lineWidth is float width) Engine.GL.LineWidth(width);

    Engine.GL.DrawElementsInstanced(primitiveType, indexCount, DrawElementsType.UnsignedInt, (void*)0, (uint)instances.Count);
    CountDrawCall();

    shader.SetBool("uInstancedTransform", false);
    shader.SetBool("uUseVertexColor", false);
  }

  // Skinned meshes draw immediately since bone matrices vary per instance. Non-skinned meshes
  // are queued and instanced at EndMode3D.
  public void DrawModel(Model model, Matrix4x4 worldMatrix, Matrix4x4[] boneMatrices)
  {
    foreach (Mesh mesh in model.Meshes)
    {
      bool skinned = mesh.IsSkinned && boneMatrices.Length > 0;

      if (!skinned)
      {
        GetMeshGroup(mesh).Add(mesh.NodeTransform * worldMatrix);
        continue;
      }

      Engine.GL.BindVertexArray(mesh.Vao);
      shader.SetMatrix("uModel", mesh.NodeTransform * worldMatrix);
      shader.SetColor("uColor", Color.White);
      shader.SetBool("uSkinned", true);
      shader.SetMatrixArray("uBones", boneMatrices);

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
      CountDrawCall();

      shader.SetBool("uTextured", false);
      shader.SetBool("uSkinned", false);
    }
  }

  List<Matrix4x4> GetMeshGroup(Mesh mesh)
  {
    if (!pendingMeshInstances.TryGetValue(mesh, out List<Matrix4x4>? group))
    {
      group = [];
      pendingMeshInstances[mesh] = group;
    }
    return group;
  }

  unsafe void FlushMeshInstances()
  {
    if (pendingMeshInstances.Count == 0) return;

    shader.SetColor("uColor", Color.White);
    shader.SetBool("uInstancedTransform", true);

    foreach ((Mesh mesh, List<Matrix4x4> transforms) in pendingMeshInstances)
    {
      Engine.GL.BindVertexArray(mesh.Vao);
      Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, mesh.InstanceVbo);
      Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, MemoryMarshal.Cast<Matrix4x4, float>(CollectionsMarshal.AsSpan(transforms)), BufferUsageARB.DynamicDraw);

      if (mesh.Texture != null)
      {
        Engine.GL.ActiveTexture(TextureUnit.Texture0);
        Engine.GL.BindTexture(TextureTarget.Texture2D, mesh.Texture.Handle);
        shader.SetInt("uTexture", 0);
        shader.SetBool("uTextured", true);
      }

      Engine.GL.DrawElementsInstanced(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, (void*)0, (uint)transforms.Count);
      CountDrawCall();

      shader.SetBool("uTextured", false);
    }

    shader.SetBool("uInstancedTransform", false);
    pendingMeshInstances.Clear();
  }

  #endregion


  #region Text Drawing

  public void DrawText(string text, float x, float y, TextStyle style) =>
    fontRenderer.DrawText(text, x, y, style, projection);

  #endregion
}
