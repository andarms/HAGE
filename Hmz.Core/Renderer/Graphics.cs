using FontStashSharp;
using FontStashSharp.Interfaces;
using Hmz.Core.Content;
using Hmz.Core.Renderer._2D;
using Hmz.Core.Renderer._3D;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Hmz.Core.Renderer;

public sealed class Graphics : IGraphics, IDisposable, IFontStashRenderer2, ITexture2DManager
{
  readonly Shader shader;
  readonly uint vao, vbo;
  readonly uint cubeVao, cubeVbo, cubeEbo;
  readonly uint dynamicVao, dynamicVbo;
  readonly uint textVao, textVbo;
  readonly Font defaultFont;

  Matrix4x4 projection;

  public Graphics(int width, int height)
  {
    string vert = EmbeddedResources.ReadText("Hmz.Core.Resources.Shaders.default.vert");
    string frag = EmbeddedResources.ReadText("Hmz.Core.Resources.Shaders.default.frag");
    shader = new Shader(vert, frag);

    FontSystem defaultFontSystem = new();
    defaultFontSystem.AddFont(EmbeddedResources.ReadBytes("Hmz.Core.Resources.Fonts.monogram-extended.ttf"));
    defaultFont = new Font(defaultFontSystem);

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

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
    }
    Engine.GL.EnableVertexAttribArray(0);

    // Position-only, re-buffered per draw call — backs the procedural 2D shapes (rectangles, circles, lines).
    dynamicVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(dynamicVao);

    dynamicVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, dynamicVbo);

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
    }
    Engine.GL.EnableVertexAttribArray(0);

    // Position + texcoord, re-buffered per glyph quad — backs FontStashSharp's text rendering.
    textVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(textVao);

    textVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, textVbo);

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
      Engine.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
    }
    Engine.GL.EnableVertexAttribArray(0);
    Engine.GL.EnableVertexAttribArray(1);

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
    Engine.GL.DeleteVertexArray(dynamicVao);
    Engine.GL.DeleteBuffer(dynamicVbo);
    Engine.GL.DeleteVertexArray(textVao);
    Engine.GL.DeleteBuffer(textVbo);
    defaultFont.Dispose();
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
    Vector2[] ring = BuildRectangleRing(x, y, width, height, radius);
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
    UploadDynamic(ToVertexData(points));
    shader.SetMatrix("uModel", Matrix4x4.Identity);
    shader.SetColor("uColor", style.Color);
    Engine.GL.LineWidth(style.Width);
    Engine.GL.DrawArrays(PrimitiveType.LineStrip, 0, (uint)points.Length);
  }

  public void DrawCircle(float centerX, float centerY, float radius, CircleStyle style)
  {
    Vector2[] ring = BuildCircleRing(centerX, centerY, radius);
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
      UploadDynamic(ToFanVertexData(center, ring));
      shader.SetMatrix("uModel", Matrix4x4.Identity);
      shader.SetColor("uColor", fill);
      Engine.GL.DrawArrays(PrimitiveType.TriangleFan, 0, (uint)(ring.Length + 2));
    }

    if (border != null)
    {
      UploadDynamic(ToVertexData(ring));
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

  static float[] ToVertexData(Vector2[] points)
  {
    float[] vertices = new float[points.Length * 3];
    for (int i = 0; i < points.Length; i++)
    {
      vertices[i * 3 + 0] = points[i].X;
      vertices[i * 3 + 1] = points[i].Y;
      vertices[i * 3 + 2] = 0f;
    }
    return vertices;
  }

  static float[] ToFanVertexData(Vector2 center, Vector2[] ring)
  {
    float[] vertices = new float[(ring.Length + 2) * 3];
    vertices[0] = center.X;
    vertices[1] = center.Y;
    vertices[2] = 0f;
    for (int i = 0; i < ring.Length; i++)
    {
      vertices[(i + 1) * 3 + 0] = ring[i].X;
      vertices[(i + 1) * 3 + 1] = ring[i].Y;
      vertices[(i + 1) * 3 + 2] = 0f;
    }
    int closing = ring.Length + 1;
    vertices[closing * 3 + 0] = ring[0].X;
    vertices[closing * 3 + 1] = ring[0].Y;
    vertices[closing * 3 + 2] = 0f;
    return vertices;
  }

  static Vector2[] BuildRectangleRing(float x, float y, float w, float h, float cornerRadius, int cornerSegments = 8)
  {
    if (cornerRadius <= 0f)
    {
      return [new(x, y), new(x + w, y), new(x + w, y + h), new(x, y + h)];
    }

    List<Vector2> points = [];
    AddArc(points, x + w - cornerRadius, y + cornerRadius, cornerRadius, 270, 360, cornerSegments);
    AddArc(points, x + w - cornerRadius, y + h - cornerRadius, cornerRadius, 0, 90, cornerSegments);
    AddArc(points, x + cornerRadius, y + h - cornerRadius, cornerRadius, 90, 180, cornerSegments);
    AddArc(points, x + cornerRadius, y + cornerRadius, cornerRadius, 180, 270, cornerSegments);
    return [.. points];
  }

  static Vector2[] BuildCircleRing(float centerX, float centerY, float radius, int segments = 32)
  {
    Vector2[] points = new Vector2[segments];
    for (int i = 0; i < segments; i++)
    {
      float angle = i / (float)segments * MathF.Tau;
      points[i] = new Vector2(centerX + radius * MathF.Cos(angle), centerY + radius * MathF.Sin(angle));
    }
    return points;
  }

  static void AddArc(List<Vector2> points, float cx, float cy, float radius, float fromDegrees, float toDegrees, int segments)
  {
    for (int i = 0; i <= segments; i++)
    {
      float degrees = fromDegrees + (toDegrees - fromDegrees) * i / segments;
      float radians = degrees * MathF.PI / 180f;
      points.Add(new Vector2(cx + radius * MathF.Cos(radians), cy + radius * MathF.Sin(radians)));
    }
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
    shader.SetMatrix("uModel", cube.GetModelMatrix());

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

  public void DrawModel(Model model)
  {
    Matrix4x4 transform = model.GetModelMatrix();

    foreach (Mesh mesh in model.Meshes)
    {
      Engine.GL.BindVertexArray(mesh.Vao);
      shader.SetMatrix("uModel", mesh.NodeTransform * transform);
      shader.SetColor("uColor", Color.White);

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
    }
  }

  #endregion


  #region Text Drawing

  public void DrawText(string text, float x, float y, TextStyle style)
  {
    FontSystem fontSystem = (style.Font ?? defaultFont).System;
    DynamicSpriteFont font = fontSystem.GetFont(style.FontSize);

    Engine.GL.Disable(EnableCap.DepthTest);
    Engine.GL.Enable(EnableCap.Blend);
    // FontStashSharp bakes its glyph atlas with premultiplied alpha (FontSystemSettings.GlyphRenderResult
    // defaults to Premultiplied), so the source factor must be One, not SrcAlpha — otherwise edges get
    // attenuated twice and blend into a soft, discolored halo instead of a crisp glyph.
    Engine.GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
    shader.Use();
    shader.SetMatrix("uProjection", projection);
    shader.SetMatrix("uView", Matrix4x4.Identity);
    shader.SetMatrix("uModel", Matrix4x4.Identity);
    shader.SetInt("uTexture", 0);
    shader.SetBool("uTextured", true);

    if (style.Outline != null)
    {
      // FontSystemEffect.Stroked dilates the glyph bitmap into a differently-sized atlas cell,
      // which shifts its bearing relative to the plain glyph and renders as a lopsided shadow
      // rather than a symmetric halo. Stamping the same undilated glyph at integer pixel offsets
      // in a ring around the origin keeps every copy pixel-identical to the fill pass, so the
      // outline lines up evenly on all sides.
      FSColor outlineColor = ToFSColor(style.Outline.Color);
      int width = Math.Max(1, (int)MathF.Round(style.Outline.Width));
      for (int oy = -width; oy <= width; oy++)
      {
        for (int ox = -width; ox <= width; ox++)
        {
          if (ox == 0 && oy == 0) continue;
          font.DrawText(this, text, new Vector2(x + ox, y + oy), outlineColor);
        }
      }
    }

    font.DrawText(this, text, new Vector2(x, y), ToFSColor(style.Color));

    shader.SetBool("uTextured", false);
    Engine.GL.Disable(EnableCap.Blend);
  }

  static FSColor ToFSColor(Color c) => new(c.R, c.G, c.B, c.A);

  // Below: the FontStashSharp backend — glyph atlas management (ITexture2DManager) and
  // per-glyph quad submission (IFontStashRenderer2). FontStashSharp calls these directly;
  // they aren't meant to be called from game code.

  ITexture2DManager IFontStashRenderer2.TextureManager => this;

  object ITexture2DManager.CreateTexture(int width, int height)
  {
    uint handle = Engine.GL.GenTexture();
    Engine.GL.BindTexture(TextureTarget.Texture2D, handle);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    unsafe
    {
      Engine.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
        (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (void*)0);
    }
    return new Texture2D(handle, width, height);
  }

  System.Drawing.Point ITexture2DManager.GetTextureSize(object texture)
  {
    Texture2D t = (Texture2D)texture;
    return new System.Drawing.Point(t.Width, t.Height);
  }

  void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
  {
    Engine.GL.BindTexture(TextureTarget.Texture2D, ((Texture2D)texture).Handle);
    unsafe
    {
      fixed (byte* ptr = data)
        Engine.GL.TexSubImage2D(TextureTarget.Texture2D, 0, bounds.X, bounds.Y,
          (uint)bounds.Width, (uint)bounds.Height, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
    }
  }

  void IFontStashRenderer2.DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
    ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
  {
    // FontStashSharp bakes the requested draw color into each vertex rather than a uniform;
    // all four corners of a glyph quad share the same color, so any one of them will do.
    shader.SetColor("uColor", new Color(topLeft.Color.R, topLeft.Color.G, topLeft.Color.B, topLeft.Color.A));

    float[] vertices =
    [
      topLeft.Position.X, topLeft.Position.Y, topLeft.Position.Z, topLeft.TextureCoordinate.X, topLeft.TextureCoordinate.Y,
      bottomLeft.Position.X, bottomLeft.Position.Y, bottomLeft.Position.Z, bottomLeft.TextureCoordinate.X, bottomLeft.TextureCoordinate.Y,
      topRight.Position.X, topRight.Position.Y, topRight.Position.Z, topRight.TextureCoordinate.X, topRight.TextureCoordinate.Y,
      topRight.Position.X, topRight.Position.Y, topRight.Position.Z, topRight.TextureCoordinate.X, topRight.TextureCoordinate.Y,
      bottomLeft.Position.X, bottomLeft.Position.Y, bottomLeft.Position.Z, bottomLeft.TextureCoordinate.X, bottomLeft.TextureCoordinate.Y,
      bottomRight.Position.X, bottomRight.Position.Y, bottomRight.Position.Z, bottomRight.TextureCoordinate.X, bottomRight.TextureCoordinate.Y,
    ];

    Engine.GL.BindVertexArray(textVao);
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, textVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.DynamicDraw);

    Engine.GL.ActiveTexture(TextureUnit.Texture0);
    Engine.GL.BindTexture(TextureTarget.Texture2D, ((Texture2D)texture).Handle);

    Engine.GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
  }

  #endregion
}
