using Hmz.Core.Renderer._2D;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Hmz.Core.Content;

// Decodes an image on the CPU and uploads it to OpenGL, exposing only the engine texture type.
static class TextureLoader
{
  public static Texture2D Upload(byte[] bytes)
  {
    ImageResult image = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

    uint handle = Engine.GL.GenTexture();
    Engine.GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    Engine.GL.BindTexture(TextureTarget.Texture2D, handle);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
    Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    unsafe
    {
      // The decoded pixels are needed only while OpenGL copies them into the texture.
      fixed (byte* ptr = image.Data)
        Engine.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
          (uint)image.Width, (uint)image.Height, 0,
          PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
    }
    Engine.GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

    return new Texture2D(handle, image.Width, image.Height);
  }
}
