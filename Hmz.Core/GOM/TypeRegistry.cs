using Properties = System.Text.Json.Nodes.JsonObject;

namespace Hmz.Core.GOM;

public class TypeRegistry<T> where T : GameObject
{
  readonly Dictionary<string, Func<Properties?, T>> factories = [];

  public void Register(string key, Func<Properties?, T> factory)
  {
    if (!factories.TryAdd(key, factory))
    {
      throw new InvalidOperationException($"Duplicate key: '{key}'");
    }
  }

  public T Create(string key, Properties? properties)
  {
    if (!factories.TryGetValue(key, out var factory))
    {
      throw new KeyNotFoundException($"Unknown type key: '{key}'");
    }
    return factory(properties);
  }

  public bool Contains(string key)
  {
    return factories.ContainsKey(key);
  }

  public IEnumerable<string> Keys => factories.Keys;
}
