using Hmz.Core.GOM;

namespace Hmz.Core.Scenes;


public class Scene
{
  public GameObjectCollection Instances { get; } = [];

  // Whether this scene reads input when it's current or sitting on the stack;
  // set via SceneManager.Push's captureInput argument. Defaults to true so plain
  // pushes/switches behave as before.
  public bool InputEnabled { get; set; } = true;

  public virtual void Initialize()
  {
    foreach (GameObject instance in Instances)
    {
      instance.Initialize();
    }
  }

  internal void HandleInput()
  {
    foreach (GameObject instance in Instances)
    {
      instance.HandleInput();
    }
  }

  public virtual void Update(float dt)
  {
    Instances.Update(dt, onAdded: i => i.Initialize(), onRemoved: i => i.Terminate());
  }

  public virtual void Draw()
  {
    // 3D visibility is resolved by the depth buffer, so only DrawOrder controls sequencing
    // here (e.g. grouping for batching), not a 2D-style Y-sort.
    var instancesOrdered = Instances.OrderBy(i => i.DrawOrder);
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
    foreach (GameObject instance in Instances)
    {
      instance.Terminate();
    }
    Instances.Clear();
  }

  public virtual bool IsBusy() => false;

  internal void RestoreSaveData()
  {
    // foreach (var instance in instances)
    // {
    //   instance.Load();
    // }
  }
}
