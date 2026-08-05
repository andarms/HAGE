using FontStashSharp;
using Hmz.Core._3D.Geometry;
using Hmz.Core.GOM;
using Hmz.Core.Renderer;
using Hmz.Core.Tilemap;

namespace Hmz.Core.Content;

public sealed class ContentManager
{
  public string ContentRoot { get; init; } = "assets/";
  public string Resolve(string path) => Path.Combine(AppContext.BaseDirectory, ContentRoot, path);
  private readonly Dictionary<string, object> cache = [];

  public Font DefaultFont { get; } = LoadEmbeddedFont();

  public Font LoadFont(string path)
  {
    string key = Resolve(path);
    if (cache.TryGetValue(key, out object? cached) && cached is Font cachedFont) return cachedFont;

    FontSystem system = new();
    system.AddFont(File.ReadAllBytes(key));
    Font font = new(system);
    cache[key] = font;
    return font;
  }

  static Font LoadEmbeddedFont()
  {
    FontSystem system = new();
    system.AddFont(EmbeddedResources.ReadBytes("Hmz.Core.Resources.Fonts.monogram-extended.ttf"));
    return new Font(system);
  }

  public Texture2D LoadTexture(string path)
  {
    string key = Resolve(path);
    if (cache.TryGetValue(key, out object? cached) && cached is Texture2D cachedTexture) return cachedTexture;

    Texture2D texture = TextureLoader.Upload(File.ReadAllBytes(key));
    cache[key] = texture;
    return texture;
  }

  public Model LoadModel(string path)
  {
    string key = Resolve(path);
    if (cache.TryGetValue(key, out object? cached) && cached is Model cachedModel) return cachedModel;

    Model model = ModelLoader.Load(path, key);
    cache[key] = model;
    return model;
  }

  // Not cached: unlike Model/Texture/Font, a TileMap is a live GameObject graph with its own
  // mutable Transform/Collider state, not a shareable immutable asset.
  public TileMap LoadTilemap(string path, TypeRegistry<Tile3D> tiles, TypeRegistry<GameObject> objects)
  {
    TilemapDocument document = TilemapDocument.Load(Resolve(path));
    return TileMap.FromDocument(document, tiles, objects);
  }

  public Shader LoadShader(string path)
  {
    throw new NotImplementedException("Shader loading is not implemented yet.");
  }

  public bool TryGet<T>(string path, out T? asset) where T : class
  {
    if (cache.TryGetValue(Resolve(path), out var obj) && obj is T typedAsset)
    {
      asset = typedAsset;
      return true;
    }
    asset = null;
    return false;
  }

  public void Unload(string path)
  {
    string key = Resolve(path);
    if (cache.TryGetValue(key, out var asset))
    {
      if (asset is IDisposable disposable)
      {
        disposable.Dispose();
      }
      cache.Remove(key);
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
