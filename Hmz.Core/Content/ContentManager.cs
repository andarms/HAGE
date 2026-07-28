using Hmz.Core.Graphics._3D;
using Hmz.Core.Renderer._2D;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Hmz.Core.Content;


public interface IContent
{
  // path is relative to the content root (e.g. "assets/") — engine resolves it
  Texture2D LoadTexture(string path);
  Model LoadModel(string path);
  Shader LoadShader(string vertexPath, string fragmentPath);
  // Font LoadFont(string path);

  bool TryGet<T>(string path, out T asset) where T : class;  // cache peek, no load
  void Unload(string path);   // free one asset + drop it from the cache
  void UnloadAll();           // called on shutdown; frees every tracked GL resource
}


public sealed class ContentManager : IContent
{
  public string ContentRoot { get; init; } = "assets/";
  public string Resolve(string path) => Path.Combine(AppContext.BaseDirectory, ContentRoot, path);
  private readonly Dictionary<string, object> cache = [];

  // inside Content.LoadTexture — the ONLY place StbImageSharp is mentioned
  public Texture2D LoadTexture(string path)
  {
    ImageResult image = ImageResult.FromMemory(          // <- Stb type, local, temporary
        File.ReadAllBytes(Resolve(path)),
        ColorComponents.RedGreenBlueAlpha);

    uint handle = Engine.GL.GenTexture();
    Engine.GL.BindTexture(TextureTarget.Texture2D, handle);
    unsafe
    {
      fixed (byte* ptr = image.Data)                     // <- Stb's pixels, handed to GL
        Engine.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
          (uint)image.Width, (uint)image.Height, 0,
          PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
    }
    // ... filter/wrap params, mipmaps ...

    return new Texture2D(handle, image.Width, image.Height);  // <- YOUR type escapes
  }

  public Model LoadModel(string path)
  {
    if (cache.TryGetValue(path, out var asset) && asset is Model model)
      return model;

    // Load the model from disk or other source
    model = new Model { Path = path }; // Placeholder for actual loading logic
    cache[path] = model;
    return model;
  }

  public Shader LoadShader(string vertexPath, string fragmentPath)
  {
    string key = $"{vertexPath}|{fragmentPath}";
    if (cache.TryGetValue(key, out var asset) && asset is Shader shader)
      return shader;

    // Load the shader from disk or other source
    shader = new Shader(vertexPath, fragmentPath); // Placeholder for actual loading logic
    cache[key] = shader;
    return shader;
  }

  public bool TryGet<T>(string path, out T asset) where T : class
  {
    if (cache.TryGetValue(path, out var obj) && obj is T typedAsset)
    {
      asset = typedAsset;
      return true;
    }
    asset = null;
    return false;
  }

  public void Unload(string path)
  {
    if (cache.TryGetValue(path, out var asset))
    {
      if (asset is IDisposable disposable)
        disposable.Dispose();
      cache.Remove(path);
    }
  }

  public void UnloadAll()
  {
    foreach (var asset in cache.Values)
    {
      if (asset is IDisposable disposable)
        disposable.Dispose();
    }
    cache.Clear();
  }
}