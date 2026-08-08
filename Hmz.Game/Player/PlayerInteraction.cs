using System.Numerics;
using Hmz.Core.Collisions;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class PlayerInteraction : GameObject
{
  const float ForwardOffset = 0.8f;

  public PlayerInteraction()
  {
    Transform.Position = Vector3.UnitZ * ForwardOffset;
    Collider = new(this)
    {
      Type = CollisionType.Trigger,
      Size = new Vector3(0.5f, 0.5f, 0.5f),
      Offset = new Vector3(0f, 0.5f, 0f),
      Layer = CollisionLayer.Player,
      Mask = CollisionLayer.All & ~CollisionLayer.Player,
    };
  }
}