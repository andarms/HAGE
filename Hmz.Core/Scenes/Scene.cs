namespace Hmz.Core.Scenes;


public class Scene
{
  readonly List<GameObject> instances = [];

  readonly Queue<GameObject> pendingInstances = new();

  readonly Queue<GameObject> pendingRemoveInstances = new();

  public IReadOnlyList<GameObject> Instances => instances.AsReadOnly();

  void FlushQueues()
  {
    while (pendingRemoveInstances.TryDequeue(out var go))
    {
      instances.Remove(go);
      go.Terminate();
    }

    while (pendingInstances.TryDequeue(out var go))
    {
      instances.Add(go);
      go.Initialize();
    }
  }

  public virtual void Initialize()
  {
    instances.ForEach(i => i.Initialize());
  }

  internal void HandleInput()
  {
    instances.ForEach(i => i.HandleInput());
  }

  public virtual void Update(float dt)
  {
    FlushQueues();

    int instanceCount = instances.Count;
    for (int i = 0; i < instanceCount; i++)
    {
      instances[i].Update(dt);
      if (instances[i].IsActive == false)
      {
        pendingRemoveInstances.Enqueue(instances[i]);
      }
    }
  }

  public virtual void Draw()
  {
    var instancesOrdered = instances.OrderBy(i => i.DrawOrder).ThenBy(i => i.WorldTransform.Position.Y);
    foreach (GameObject instance in instancesOrdered)
    {
      instance.Draw();
      if (Engine.DebugMode) instance.Debug();
    }
  }

  // public virtual void DrawUI()
  // {
  //   var instancesOrdered = instances.OrderBy(i => i.DrawOrder).ThenBy(i => i.Position.Y);
  //   foreach (GameObject instance in instancesOrdered)
  //   {
  //     instance.DrawUI();
  //   }
  // }

  public virtual void Pause() { }

  public virtual void Resume() { }

  public virtual void Terminate()
  {
    pendingInstances.Clear();
    pendingRemoveInstances.Clear();

    instances.ForEach(i => i.Terminate());
    instances.Clear();
  }

  public virtual bool IsBusy() => false;

  public void Add(GameObject obj) => pendingInstances.Enqueue(obj);

  public void Remove(GameObject obj) => pendingRemoveInstances.Enqueue(obj);

  internal void RestoreSaveData()
  {
    // foreach (var instance in instances)
    // {
    //   instance.Load();
    // }
  }
}
