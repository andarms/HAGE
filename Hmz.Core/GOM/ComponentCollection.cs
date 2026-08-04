namespace Hmz.Core.GOM;

public interface IReadOnlyComponentCollection
{
  bool Has<T>() where T : Component;
  T? Get<T>() where T : Component;
  T Require<T>() where T : Component;
}

public class ComponentCollection(GameObject owner) : IReadOnlyComponentCollection
{
  readonly Dictionary<Type, Component> components = [];
  readonly Queue<Component> pendingAdd = new();
  readonly Queue<Type> pendingRemove = new();

  public void Add(Component component)
  {
    ArgumentNullException.ThrowIfNull(component);

    Type type = component.GetType();
    if (components.ContainsKey(type) || pendingAdd.Any(c => c.GetType() == type))
    {
      throw new InvalidOperationException($"A component of type {type.Name} is already attached.");
    }

    component.Attach(owner);
    pendingAdd.Enqueue(component);
  }

  public bool Remove<T>() where T : Component
  {
    Type type = typeof(T);
    if (!components.ContainsKey(type))
    {
      return false;
    }

    pendingRemove.Enqueue(type);
    return true;
  }


  public T? Get<T>() where T : Component
  {
    components.TryGetValue(typeof(T), out Component? component);
    return component as T;
  }

  public T Require<T>() where T : Component
  {
    T? component = Get<T>() ?? throw new InvalidOperationException($"Component of type {typeof(T).Name} is not attached.");
    return component;
  }

  public bool Has<T>() where T : Component => components.ContainsKey(typeof(T));

  public IReadOnlyCollection<Component> All => components.Values;

  public void Flush()
  {
    while (pendingRemove.TryDequeue(out Type? type))
    {
      if (components.Remove(type, out Component? removed))
      {
        removed.Detach();
      }
    }

    while (pendingAdd.TryDequeue(out Component? component))
    {
      components[component.GetType()] = component;
    }
  }

  public void Initialize()
  {
    foreach (Component component in components.Values)
    {
      component.Initialize();
    }
  }

  public void HandleInput()
  {
    foreach (Component component in components.Values)
    {
      component.HandleInput();
    }
  }


  public void Update(float dt)
  {
    Flush();
    foreach (Component component in components.Values)
    {
      component.Update(dt);
    }
  }

  public void Draw()
  {
    foreach (Component component in components.Values)
    {
      component.Draw();
    }
  }

  public void Debug()
  {
    foreach (Component component in components.Values)
    {
      component.Debug();
    }
  }

  public void Terminate()
  {
    foreach (Component component in components.Values)
    {
      component.Terminate();
      component.Detach();
    }
    components.Clear();
    pendingAdd.Clear();
    pendingRemove.Clear();
  }
}
