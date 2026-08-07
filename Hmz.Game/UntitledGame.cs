using Hmz.Core;
using Hmz.Core.Hosting;
using Hmz.Core.Input;
using Hmz.Editor;

namespace Hmz.Game;

public class UntitledGame : Core.Hosting.Game
{
  protected override void LoadScenes()
  {
    MapActions();
    RegisterGameObjectTypes();
    Engine.Scenes.Add(new GameplayScene());
    Engine.Scenes.Add(new EditorScene());
    Engine.Scenes.Add(new TilemapEditorScene());
    Engine.Scenes.Add(new PauseScene());
    Engine.Scenes.SetStartScene<GameplayScene>();
  }

  protected override void Update(float dt)
  {
    if (Engine.Input.IsKeyJustPressed(Key.F2))
    {
      if (Engine.Scenes.Current is EditorScene)
      {
        Engine.Scenes.Pop();
      }
      else
      {
        Engine.Scenes.Push<EditorScene>(false);
      }
    }

    if (Engine.Input.IsKeyJustPressed(Key.F3))
    {
      if (Engine.Scenes.Current is TilemapEditorScene)
      {
        Engine.Scenes.SwitchTo<GameplayScene>();
      }
      else if (Engine.Scenes.Current is GameplayScene)
      {
        Engine.Scenes.SwitchTo<TilemapEditorScene>();
      }
    }

    base.Update(dt);
  }

  void RegisterGameObjectTypes()
  {
    Engine.GameObjectRegistry.Tiles.Register("Box1x1", props => new Box1x1());
    Engine.GameObjectRegistry.Tiles.Register("Box1x2", props => new Box1x2());
    Engine.GameObjectRegistry.Tiles.Register("Box1x3", props => new Box1x3());
    Engine.GameObjectRegistry.Tiles.Register("Box2x1", props => new Box2x1());
    Engine.GameObjectRegistry.Tiles.Register("Box2x2", props => new Box2x2());
    Engine.GameObjectRegistry.Tiles.Register("Box2x3", props => new Box2x3());
    Engine.GameObjectRegistry.Tiles.Register("Box3x1", props => new Box3x1());
    Engine.GameObjectRegistry.Tiles.Register("Box3x2", props => new Box3x2());
    Engine.GameObjectRegistry.Tiles.Register("Box3x3", props => new Box3x3());

    Engine.GameObjectRegistry.Objects.Register("Tree", props => new Tree());
    Engine.GameObjectRegistry.Objects.Register("Crate", props => new Crate());
  }

  public void MapActions()
  {
    Engine.Input.AddBinding("move_up", Key.UpArrow);
    Engine.Input.AddBinding("move_down", Key.DownArrow);
    Engine.Input.AddBinding("move_left", Key.LeftArrow);
    Engine.Input.AddBinding("move_right", Key.RightArrow);

    Engine.Input.AddBinding("camera_left", Key.Q);
    Engine.Input.AddBinding("camera_right", Key.E);
    Engine.Input.AddBinding("camera_up", Key.R);
    Engine.Input.AddBinding("camera_down", Key.F);
    Engine.Input.AddBinding("camera_left", GamepadButton.DPadLeft);
    Engine.Input.AddBinding("camera_right", GamepadButton.DPadRight);
    Engine.Input.AddBinding("camera_up", GamepadButton.DPadUp);
    Engine.Input.AddBinding("camera_down", GamepadButton.DPadDown);

    Engine.Input.AddBinding("action_1", Key.Z);
    Engine.Input.AddBinding("action_2", Key.X);
    Engine.Input.AddBinding("action_3", Key.C);

    Engine.Input.AddBinding("action_1", GamepadButton.A);
    Engine.Input.AddBinding("action_2", GamepadButton.B);
    Engine.Input.AddBinding("action_3", GamepadButton.Y);

    Engine.Input.AddBinding("pause", Key.Escape);
    Engine.Input.AddBinding("pause", Key.P);
    Engine.Input.AddBinding("confirm", Key.Enter);

    Engine.Input.AddBinding("pause", GamepadButton.Start);
    Engine.Input.AddBinding("confirm", GamepadButton.A);
  }

}
