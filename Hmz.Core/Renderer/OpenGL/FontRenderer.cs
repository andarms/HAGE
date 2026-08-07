using FontStashSharp;
using FontStashSharp.Interfaces;
using Hmz.Core.Content;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;
using TextStyle = Hmz.Core.Renderer.Styles.TextStyle;

namespace Hmz.Core.Renderer.OpenGL;

// FontStashSharp's rendering backend: owns the glyph atlas texture (ITexture2DManager) and
// submits each glyph quad to GL (IFontStashRenderer2). Kept separate from GLGraphics because
// it's a self-contained adapter for a third-party library, not a 2D/3D primitive drawer.
sealed class FontRenderer : IFontStashRenderer2, ITexture2DManager, IDisposable
{
  readonly Shader shader;
  readonly uint textVao, textVbo;
  readonly Font defaultFont;

  // Glyph quads for the current DrawText call, flushed as one draw.
  readonly List<float> pendingVertices = [];
  object? pendingTexture;

  const int FloatsPerVertex = 9; // pos(3) + texcoord(2) + color(4)

  public int DrawCallCount { get; private set; }
  public void ResetDrawCallCount() => DrawCallCount = 0;

  public FontRenderer(Shader shader)
  {
    this.shader = shader;

    FontSystem defaultFontSystem = new();
    defaultFontSystem.AddFont(EmbeddedResources.ReadBytes("Hmz.Core.Resources.Fonts.monogram-extended.ttf"));
    defaultFont = new Font(defaultFontSystem);

    // Position + texcoord + color.
    textVao = Engine.GL.GenVertexArray();
    Engine.GL.BindVertexArray(textVao);

    textVbo = Engine.GL.GenBuffer();
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, textVbo);

    unsafe
    {
      Engine.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, FloatsPerVertex * sizeof(float), (void*)0);
      Engine.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, FloatsPerVertex * sizeof(float), (void*)(3 * sizeof(float)));
      Engine.GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, FloatsPerVertex * sizeof(float), (void*)(5 * sizeof(float)));
    }
    Engine.GL.EnableVertexAttribArray(0);
    Engine.GL.EnableVertexAttribArray(1);
    Engine.GL.EnableVertexAttribArray(8);
  }

  public void Dispose()
  {
    Engine.GL.DeleteVertexArray(textVao);
    Engine.GL.DeleteBuffer(textVbo);
    defaultFont.Dispose();
  }

  public Vector2 MeasureText(string text, TextStyle style)
  {
    FontSystem fontSystem = (style.Font ?? defaultFont).System;
    DynamicSpriteFont font = fontSystem.GetFont(style.FontSize);
    return font.MeasureString(text);
  }

  public void DrawText(string text, float x, float y, TextStyle style, Matrix4x4 projection)
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
    shader.SetBool("uUseVertexColor", true);

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

    FlushPending();

    shader.SetBool("uUseVertexColor", false);
    shader.SetBool("uTextured", false);
    Engine.GL.Disable(EnableCap.Blend);
  }

  void FlushPending()
  {
    if (pendingVertices.Count == 0) return;

    Engine.GL.BindVertexArray(textVao);
    Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, textVbo);
    Engine.GL.BufferData(BufferTargetARB.ArrayBuffer, CollectionsMarshal.AsSpan(pendingVertices), BufferUsageARB.DynamicDraw);

    Engine.GL.ActiveTexture(TextureUnit.Texture0);
    Engine.GL.BindTexture(TextureTarget.Texture2D, ((Texture2D)pendingTexture!).Handle);

    Engine.GL.DrawArrays(PrimitiveType.Triangles, 0, (uint)(pendingVertices.Count / FloatsPerVertex));
    DrawCallCount++;

    pendingVertices.Clear();
    pendingTexture = null;
  }

  static FSColor ToFSColor(Color c) => new(c.R, c.G, c.B, c.A);

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
    // Flush before switching textures so each draw call samples only one texture.
    if (pendingTexture != null && !ReferenceEquals(pendingTexture, texture))
    {
      FlushPending();
    }
    pendingTexture = texture;

    // FontStashSharp bakes the requested draw color into each vertex rather than a uniform;
    // all four corners of a glyph quad share the same color, so any one of them will do.
    float r = topLeft.Color.R / 255f, g = topLeft.Color.G / 255f, b = topLeft.Color.B / 255f, a = topLeft.Color.A / 255f;

    AppendVertex(topLeft, r, g, b, a);
    AppendVertex(bottomLeft, r, g, b, a);
    AppendVertex(topRight, r, g, b, a);
    AppendVertex(topRight, r, g, b, a);
    AppendVertex(bottomLeft, r, g, b, a);
    AppendVertex(bottomRight, r, g, b, a);
  }

  void AppendVertex(VertexPositionColorTexture v, float r, float g, float b, float a)
  {
    pendingVertices.Add(v.Position.X);
    pendingVertices.Add(v.Position.Y);
    pendingVertices.Add(v.Position.Z);
    pendingVertices.Add(v.TextureCoordinate.X);
    pendingVertices.Add(v.TextureCoordinate.Y);
    pendingVertices.Add(r);
    pendingVertices.Add(g);
    pendingVertices.Add(b);
    pendingVertices.Add(a);
  }
}
