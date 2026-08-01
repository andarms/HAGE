using Hmz.Core.Renderer;

namespace Hmz.Core;

public class Game
{
  protected internal virtual void LoadScenes() { }
  protected internal virtual void Initialize() { }
  protected internal virtual void Update(float dt) { }

  // Runs scissored to the fitted viewport rect, before the scene draws on
  // top of it - override to clear the game background to a different color.
  protected internal virtual void Draw()
  {
    Engine.Graphics.Clear(Color.CornflowerBlue);
  }
}
