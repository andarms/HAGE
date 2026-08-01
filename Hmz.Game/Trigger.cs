using System.Numerics;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;

namespace Hmz.Game;

public class Trigger : GameObject
{
  public Trigger()
  {
    Transform.Position = new Vector3(-2f, 0f, 2f);
    Collider = new(this)
    {
      Type = CollisionType.Trigger,
      Size = new Vector3(1f, 2f, 1f),
      Offset = new Vector3(0f, 1f, 0f),
      Layer = CollisionLayer.Environment,
      Mask = CollisionLayer.All & ~CollisionLayer.Environment,
      OnCollisionEnter = collision => Console.WriteLine($"[Trigger] {collision.Other.GetType().Name} entered"),
      OnCollisionExit = collision => Console.WriteLine($"[Trigger] {collision.Other.GetType().Name} exited"),
    };
  }
}
