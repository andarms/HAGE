using System.Collections;

namespace Hmz.Core.GOM;

public interface IReadOnlyGameObjectCollection : IReadOnlyList<GameObject>
{
  bool Contains(GameObject item);
}

public class GameObjectCollection : IReadOnlyGameObjectCollection
{
  readonly List<GameObject> items = [];
  readonly Queue<GameObject> pendingAdd = new();
  readonly Queue<GameObject> pendingRemove = new();

  public int Count => items.Count;
  public GameObject this[int index] => items[index];

  public bool Contains(GameObject item) => items.Contains(item);

  public virtual void Add(GameObject item)
  {
    ArgumentNullException.ThrowIfNull(item);
    pendingAdd.Enqueue(item);
  }

  public virtual void Remove(GameObject item)
  {
    ArgumentNullException.ThrowIfNull(item);
    pendingRemove.Enqueue(item);
  }

  public void Flush(Action<GameObject>? onAdded = null, Action<GameObject>? onRemoved = null)
  {
    while (pendingRemove.TryDequeue(out GameObject? item))
    {
      if (items.Remove(item))
      {
        onRemoved?.Invoke(item);
      }
    }

    while (pendingAdd.TryDequeue(out GameObject? item))
    {
      items.Add(item);
      onAdded?.Invoke(item);
    }
  }

  public void Clear()
  {
    items.Clear();
    pendingAdd.Clear();
    pendingRemove.Clear();
  }

  public IEnumerator<GameObject> GetEnumerator() => items.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public void PurgeInactiveItems()
  {
    foreach (var item in items)
    {
      if (!item.IsActive && !pendingRemove.Contains(item))
      {
        pendingRemove.Enqueue(item);
      }
    }
  }

  public void Update(float dt, Action<GameObject>? onAdded = null, Action<GameObject>? onRemoved = null)
  {
    Flush(onAdded, onRemoved);

    int count = items.Count;
    for (int i = 0; i < count; i++)
    {
      GameObject item = items[i];
      if (item.IsActive)
      {
        item.Update(dt);
      }
    }
    PurgeInactiveItems();
  }
}
