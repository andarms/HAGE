using Hmz.Core.Renderer;

namespace Hmz.Core.Hosting;

public class Game
{
  public Color BackgroundColor { get; set; } = Color.CornflowerBlue;

  protected internal virtual void LoadScenes() { }
  protected internal virtual void Initialize() => Engine.Initialize();
  protected internal virtual void Update(float dt) => Engine.Update(dt);
  protected internal virtual void Draw() => Engine.Draw();
  protected internal virtual void Terminate() => Engine.Terminate();
}
