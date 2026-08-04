using System;
using System.Collections.Generic;
namespace Hmz.Core.Scenes;

public class SceneManager
{
  Scene? current = null;
  public Scene? Current => current;

  // The scene directly beneath Current, e.g. the gameplay scene paused behind the editor overlay.
  public Scene? Below => sceneStack.Count > 0 ? sceneStack.Peek() : null;

  Scene? nextScene = null;
  Scene? startScene = null;

  readonly Stack<Scene> sceneStack = new();
  readonly Dictionary<Type, Scene> scenes = new();


  public void Initialize()
  {
    if (Current == null && startScene != null)
    {
      current = startScene;
    }
    else
    {
      throw new InvalidOperationException("A start scene must be selected before initialization.");
    }
    Current?.Initialize();
  }

  public void Update(float dt)
  {
    // Stacked scenes keep updating (so animation/state behind an overlay like the
    // editor stays live), but only one scene reads input each frame: the current
    // scene if it wants input, otherwise the nearest scene below it that does.
    foreach (var scene in sceneStack)
    {
      scene.Update(dt);
    }

    GetInputScene()?.HandleInput();
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

  public void SetStartScene<T>() where T : Scene
  {
    startScene = Get<T>();
  }

  public void Push<T>(bool captureInput = true) where T : Scene
  {
    Scene newScene = Get<T>();
    newScene.InputEnabled = captureInput;
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

  // Walks from the current scene down through the stack and returns the first one
  // that wants input, so a scene pushed with captureInput:false lets input pass
  // through to whatever is beneath it.
  Scene? GetInputScene()
  {
    if (current != null && current.InputEnabled) return current;

    foreach (var scene in sceneStack)
    {
      if (scene.InputEnabled) return scene;
    }

    return null;
  }
}
