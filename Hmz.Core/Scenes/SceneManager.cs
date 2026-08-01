using System;
using System.Collections.Generic;
using System.Linq;
namespace Hmz.Core.Scenes;

public class SceneManager
{
  Scene? current = null;
  public Scene? Current => current;
  Scene? nextScene = null;

  readonly Stack<Scene> sceneStack = new();
  readonly Dictionary<Type, Scene> scenes = new();


  public void Initialize()
  {
    if (Current == null && scenes.Count > 0)
    {
      current = scenes.First().Value;
    }
    else
    {
      throw new("Not Scenes avaible");
    }
    Current?.Initialize();
  }

  internal void HandleInput()
  {
    Current?.HandleInput();
  }

  public void Update(float dt)
  {
    Current?.Update(dt);
    if (nextScene != null && current != null && !current.IsBusy())
    {
      current?.Terminate();
      current = nextScene;
      current?.Initialize();
      nextScene = null;
    }
  }

  public void Draw()
  {
    foreach (var scene in sceneStack)
    {
      scene.Draw();
    }
    Current?.Draw();
  }

  // public void DrawUI()
  // {
  //   foreach (var scene in sceneStack)
  //   {
  //     scene.DrawUI();
  //   }
  //   Current?.DrawUI();
  // }

  public void Pause()
  {
    Current?.Pause();
  }

  public void Resume()
  {
    Current?.Resume();
  }

  public void Terminate() { Current?.Terminate(); }

  public void Add<T>(T scene) where T : Scene
  {
    scenes.Add(typeof(T), scene);
  }

  public void Push<T>() where T : Scene
  {
    Scene newScene = Get<T>();
    if (current != null)
    {
      current.Pause();
      sceneStack.Push(current);
    }
    current = newScene;
    current?.Initialize();
  }

  public void Pop()
  {
    if (sceneStack.Count > 0)
    {
      current?.Terminate();
      var scene = sceneStack.Pop();
      current = scene;
      current.Resume();

    }
    else
    {
      Console.WriteLine("Not scenes on stack");
    }
  }

  public void SwitchTo<T>() where T : Scene
  {
    Scene nextScene = Get<T>();
    this.nextScene = nextScene;
    if (Current == null)
    {
      current = nextScene;
      current.Initialize();
      this.nextScene = null;
    }
  }

  Scene Get<T>()
  {
    if (!scenes.TryGetValue(typeof(T), out var scene))
    {
      throw new IndexOutOfRangeException($"Scene of {typeof(T)} doesn't exint in avialbes scenes");
    }
    return scene;
  }
}
