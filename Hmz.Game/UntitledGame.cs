using Hmz.Core;
using Hmz.Core.Hosting;
using Hmz.Core.Input;

namespace Hmz.Game;

public class UntitledGame : Core.Hosting.Game
{
  protected override void LoadScenes()
  {
    MapActions();
    Engine.Scenes.Add(new GameplayScene());
  }


  public void MapActions()
  {
    Engine.Input.AddBinding("move_up", Key.UpArrow);
    Engine.Input.AddBinding("move_down", Key.DownArrow);
    Engine.Input.AddBinding("move_left", Key.LeftArrow);
    Engine.Input.AddBinding("move_right", Key.RightArrow);

    Engine.Input.AddBinding("action_1", Key.Z);
    Engine.Input.AddBinding("action_2", Key.X);
    Engine.Input.AddBinding("action_3", Key.C);

    Engine.Input.AddBinding("pause", Key.Escape);
    Engine.Input.AddBinding("confirm", Key.Enter);
  }

}
