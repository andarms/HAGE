using System.Numerics;
using Hmz.Core.GOM;

namespace Hmz.Core.Collisions;

public class Collider(GameObject owner)
{
  public GameObject Owner { get; } = owner;

  public HashSet<GameObject> CollidingWith { get; } = [];

  public Action<Collision>? OnCollisionEnter { get; set; }
  public Action<Collision>? OnCollisionStay { get; set; }
  public Action<Collision>? OnCollisionExit { get; set; }

  public Vector3 Size { get; set; } = Vector3.One;
  public Vector3 Offset { get; set; } = Vector3.Zero;

  public bool IsActive { get; set; } = true;

  public bool IsSolid { get; set; } = true;

  public CollisionLayer Layer { get; set; } = CollisionLayer.All;

  public CollisionLayer Mask { get; set; } = CollisionLayer.All;

  public virtual CollisionBox Bounds(Vector3 position)
  {
    Vector3 center = position + Offset;
    Vector3 half = Size / 2f;
    return new CollisionBox(center - half, center + half);
  }

  public virtual bool CanCollideWith(Collider other) => (Mask & other.Layer) != 0;
}
