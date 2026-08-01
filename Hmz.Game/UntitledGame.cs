using Hmz.Core;
using Hmz.Core.Hosting;

namespace Hmz.Game;

public class UntitledGame : Core.Hosting.Game
{
  protected override void LoadScenes()
  {
    Engine.Scenes.Add(new GameplayScene());
  }
}
