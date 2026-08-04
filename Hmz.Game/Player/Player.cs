using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class Player : GameObject
{
  public Player()
  {
    Transform.Position = new Vector3(0f, 0f, 0f);
    Collider = new(this)
    {
      Size = new Vector3(1f, 2f, 1f),
      Offset = new Vector3(0f, 1f, 0f),
      Layer = CollisionLayer.Player,
      Mask = CollisionLayer.All & ~CollisionLayer.Player,
    };

  }

  public override void Initialize()
  {
    base.Initialize();

    Model model = Engine.Content.LoadModel("models/player.gltf");
    ModelRenderer renderer = new(model);
    Components.Add(renderer);
    Components.Add(new Movement());
    renderer.Play("walk");

    Children.Add(new PlayerInteraction());
  }
}


public class PlayerInteraction : GameObject
{
  const float ForwardOffset = 1f;

  public PlayerInteraction()
  {
    Transform.Position = Vector3.UnitZ * ForwardOffset;
    Collider = new(this)
    {
      Type = CollisionType.Trigger,
      Size = new Vector3(1f, 1f, 1f),
      Offset = new Vector3(0f, 0.5f, 0f),
      Layer = CollisionLayer.Player,
      Mask = CollisionLayer.All & ~CollisionLayer.Player,
      OnCollisionEnter = collision => Console.WriteLine($"[Trigger] {collision.Other.GetType().Name} entered"),
      OnCollisionExit = collision => Console.WriteLine($"[Trigger] {collision.Other.GetType().Name} exited"),
    };
  }

}