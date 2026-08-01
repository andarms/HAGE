using Hmz.Core;
using Hmz.Core.Renderer;

namespace Hmz.Game;

public class UntitledGame : Core.Game
{
  protected override void LoadScenes()
  {
    Engine.Scenes.Add(new GameplayScene());
  }

  protected override void Draw()
  {
    Engine.Graphics.Clear(Color.CornflowerBlue);
  }
}
