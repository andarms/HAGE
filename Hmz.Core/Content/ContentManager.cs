using FontStashSharp;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer._2D;
using Hmz.Core.Renderer._3D;

namespace Hmz.Core.Content;

public sealed class ContentManager
{
  public string ContentRoot { get; init; } = "assets/";
  public string Resolve(string path) => Path.Combine(AppContext.BaseDirectory, ContentRoot, path);
  private readonly Dictionary<string, object> cache = [];

  public Font DefaultFont { get; } = LoadEmbeddedFont();

  public Font LoadFont(string path)
  {
    FontSystem system = new();
    system.AddFont(File.ReadAllBytes(Resolve(path)));
    return new Font(system);
  }

  static Font LoadEmbeddedFont()
  {
    FontSystem system = new();
    system.AddFont(EmbeddedResources.ReadBytes("Hmz.Core.Resources.Fonts.monogram-extended.ttf"));
    return new Font(system);
  }

  public Texture2D LoadTexture(string path) => TextureLoader.Upload(File.ReadAllBytes(Resolve(path)));

  public Model LoadModel(string path) => ModelLoader.Load(path, Resolve(path));

  public Shader LoadShader(string path)
  {
    throw new NotImplementedException("Shader loading is not implemented yet.");
  }

  public bool TryGet<T>(string path, out T? asset) where T : class
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
      {
        disposable.Dispose();
      }
      cache.Remove(path);
    }
  }

  public void UnloadAll()
  {
    foreach (var asset in cache.Values)
    {
      if (asset is IDisposable disposable)
      {
        disposable.Dispose();
      }
    }
    cache.Clear();
  }
}
