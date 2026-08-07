using Hmz.Core._2D;
using Hmz.Core.GOM;
using Hmz.Core.UI;
using System.Numerics;

namespace Hmz.Core.Scenes;


public class Scene
{
  public GameObjectCollection Instances { get; } = [];
  public CanvasLayer Canvas { get; } = [];

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
    foreach (GameObject instance in Canvas)
    {
      instance.Initialize();
    }
  }

  internal void HandleInput()
  {
    Engine.Input.ResetMouseCapture();

    foreach (GameObject instance in Canvas)
    {
      instance.HandleInput();
    }
    foreach (GameObject instance in Instances)
    {
      instance.HandleInput();
    }
  }

  public virtual void Update(float dt)
  {
    Instances.Update(dt, onAdded: i => i.Initialize(), onRemoved: i => i.Terminate());
    UpdateCanvasLayout();
    Canvas.Update(dt, onAdded: i => i.Initialize(), onRemoved: i => i.Terminate());
  }

  void UpdateCanvasLayout()
  {
    Vector2 viewport = new(Engine.Viewport.LogicalWidth, Engine.Viewport.LogicalHeight);
    foreach (UIElement root in Canvas.OfType<UIElement>())
    {
      root.Measure(viewport);
      root.Arrange(new Rectangle(0f, 0f, viewport.X, viewport.Y));
    }
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

  public virtual void DrawCanvas()
  {
    Engine.Graphics.StartMode2D(UIElement.CanvasCamera);
    var canvasOrdered = Canvas.OrderBy(i => i.DrawOrder);
    foreach (GameObject instance in canvasOrdered)
    {
      instance.Draw();
    }
    Engine.Graphics.EndMode2D();
  }

  public virtual void Pause() { }

  public virtual void Resume() { }

  public virtual void Terminate()
  {
    foreach (GameObject instance in Instances)
    {
      instance.Terminate();
    }
    Instances.Clear();
    foreach (GameObject instance in Canvas)
    {
      instance.Terminate();
    }
    Canvas.Clear();
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
