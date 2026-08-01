using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;
using Hmz.Core.Renderer._3D;

namespace Hmz.Game;

public class Player : GameObject
{
  public Player()
  {
    Transform.Position = new Vector3(0f, 0f, 0f);
    Transform.Scale = new Vector3(1f, 1f, 1f);
  }

  public override void Initialize()
  {
    Model model = Engine.Content.LoadModel("models/player.gltf");
    ModelRenderer renderer = new(model);
    Components.Add(renderer);
    renderer.Play("walk");
  }
}
