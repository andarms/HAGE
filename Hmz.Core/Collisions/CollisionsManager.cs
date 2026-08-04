using System.Numerics;
using Hmz.Core.GOM;

namespace Hmz.Core.Collisions;

public class CollisionsManager
{
  const float SkinWidth = 0.001f;

  readonly List<GameObject> collisionObjects = [];
  readonly HashSet<GameObject> registered = [];
  readonly Dictionary<GameObject, HashSet<GameObject>> previousColliding = [];

  readonly List<GameObject> activeObjects = [];
  readonly List<CollisionBox> activeBounds = [];
  readonly List<GameObject> eventBuffer = [];

  public void Register(GameObject obj)
  {
    if (!registered.Add(obj)) return;

    collisionObjects.Add(obj);
    previousColliding[obj] = [];
  }

  public void Unregister(GameObject obj)
  {
    if (!registered.Remove(obj)) return;

    collisionObjects.Remove(obj);
    previousColliding.Remove(obj);

    foreach (var other in collisionObjects)
    {
      var otherCollider = other.Collider;
      if (otherCollider?.CollidingWith.Remove(obj) != true) continue;

      otherCollider.OnCollisionExit?.Invoke(new Collision(other, obj, Direction.None));
      previousColliding[other].Remove(obj);
    }

    obj.Collider?.CollidingWith.Clear();
  }

  public static bool TryGetCollisionInfo(GameObject source, GameObject other, out Collision collisionInfo)
  {
    collisionInfo = default;

    var sourceCollider = source.Collider;
    var otherCollider = other.Collider;

    if (!source.IsActive || !other.IsActive) return false;
    if (sourceCollider?.IsActive != true || otherCollider?.IsActive != true) return false;
    if (!CanPair(sourceCollider, otherCollider)) return false;

    var sourceBounds = sourceCollider.Bounds(source.GlobalPosition);
    var otherBounds = otherCollider.Bounds(other.GlobalPosition);

    if (!sourceBounds.Intersects(otherBounds)) return false;

    collisionInfo = new Collision(source, other, GetCollisionSide(sourceBounds, otherBounds));
    return true;
  }

  public Vector3 MoveAndCollide(GameObject gameObject, Vector3 targetPosition)
  {
    var collider = gameObject.Collider;
    if (collider?.IsActive != true) return targetPosition;

    var resolvedPosition = gameObject.Transform.Position;
    var movement = targetPosition - resolvedPosition;

    resolvedPosition = ResolveAxis(gameObject, collider, resolvedPosition, movement.X, Axis.X);
    resolvedPosition = ResolveAxis(gameObject, collider, resolvedPosition, movement.Z, Axis.Z);
    resolvedPosition = ResolveAxis(gameObject, collider, resolvedPosition, movement.Y, Axis.Y);

    return resolvedPosition;
  }

  Vector3 ResolveAxis(GameObject source, Collider sourceCollider, Vector3 position, float movement, Axis axis)
  {
    if (movement == 0) return position;

    var moved = SetAxis(position, axis, GetAxis(position, axis) + movement);
    var bounds = sourceCollider.Bounds(moved);

    foreach (var other in collisionObjects)
    {
      if (ReferenceEquals(other, source) || !other.IsActive) continue;

      var otherCollider = other.Collider;
      if (otherCollider?.IsActive != true || otherCollider.Type != CollisionType.Solid) continue;
      if (!CanPair(sourceCollider, otherCollider)) continue;

      var otherBounds = otherCollider.Bounds(other.GlobalPosition);
      if (!bounds.Intersects(otherBounds)) continue;

      var correction = movement > 0
        ? GetAxis(otherBounds.Min, axis) - GetAxis(bounds.Max, axis) - SkinWidth
        : GetAxis(otherBounds.Max, axis) - GetAxis(bounds.Min, axis) + SkinWidth;

      moved = SetAxis(moved, axis, GetAxis(moved, axis) + correction);
      bounds = sourceCollider.Bounds(moved);
    }

    return moved;
  }

  public void UpdateCollisions()
  {
    activeObjects.Clear();
    activeBounds.Clear();

    foreach (var obj in collisionObjects)
    {
      if (!obj.IsActive || obj.Collider?.IsActive != true) continue;

      activeObjects.Add(obj);
      activeBounds.Add(obj.Collider.Bounds(obj.GlobalPosition));
    }

    foreach (var obj in activeObjects)
    {
      var collider = obj.Collider!;
      var previous = previousColliding[obj];

      previous.Clear();
      previous.UnionWith(collider.CollidingWith);
      collider.CollidingWith.Clear();
    }

    for (int i = 0; i < activeObjects.Count; i++)
    {
      var colliderA = activeObjects[i].Collider!;

      for (int j = i + 1; j < activeObjects.Count; j++)
      {
        var colliderB = activeObjects[j].Collider!;

        if (!CanPair(colliderA, colliderB)) continue;
        if (!activeBounds[i].Intersects(activeBounds[j])) continue;

        colliderA.CollidingWith.Add(activeObjects[j]);
        colliderB.CollidingWith.Add(activeObjects[i]);
      }
    }

    for (int i = 0; i < activeObjects.Count; i++)
    {
      var objectA = activeObjects[i];
      var colliderA = objectA.Collider!;
      var sourceBounds = activeBounds[i];
      var previous = previousColliding[objectA];

      // Snapshot before invoking, so a callback may safely unregister anything.
      eventBuffer.Clear();
      eventBuffer.AddRange(colliderA.CollidingWith);

      foreach (var other in eventBuffer)
      {
        if (other.Collider is not { } otherCollider) continue;

        var side = GetCollisionSide(sourceBounds, otherCollider.Bounds(other.GlobalPosition));
        var collision = new Collision(objectA, other, side);

        if (previous.Contains(other))
        {
          colliderA.OnCollisionStay?.Invoke(collision);
        }
        else
        {
          colliderA.OnCollisionEnter?.Invoke(collision);
        }
      }

      eventBuffer.Clear();
      foreach (var other in previous)
      {
        if (!colliderA.CollidingWith.Contains(other)) eventBuffer.Add(other);
      }

      foreach (var other in eventBuffer)
      {
        // The pair is already apart, so there is no meaningful side to report.
        colliderA.OnCollisionExit?.Invoke(new Collision(objectA, other, Direction.None));
      }
    }
  }

  public List<GameObject> GetNearbyCollisions(GameObject target, CollisionBox area)
  {
    var collisions = new List<GameObject>();
    var targetCollider = target.Collider;

    foreach (var obj in collisionObjects)
    {
      if (ReferenceEquals(obj, target) || !obj.IsActive || obj.Collider?.IsActive != true) continue;
      if (targetCollider != null && !CanPair(targetCollider, obj.Collider)) continue;
      if (obj.Collider.Bounds(obj.GlobalPosition).Intersects(area)) collisions.Add(obj);
    }

    return collisions;
  }

  static bool CanPair(Collider a, Collider b) => a.CanCollideWith(b) && b.CanCollideWith(a);

  /// <summary>Which side of <paramref name="sourceBounds"/> the other box sits against.</summary>
  static Direction GetCollisionSide(CollisionBox sourceBounds, CollisionBox otherBounds)
  {
    var delta = (otherBounds.Min + otherBounds.Max) / 2 - (sourceBounds.Min + sourceBounds.Max) / 2;
    var overlap = new Vector3(
      MathF.Min(sourceBounds.Max.X, otherBounds.Max.X) - MathF.Max(sourceBounds.Min.X, otherBounds.Min.X),
      MathF.Min(sourceBounds.Max.Y, otherBounds.Max.Y) - MathF.Max(sourceBounds.Min.Y, otherBounds.Min.Y),
      MathF.Min(sourceBounds.Max.Z, otherBounds.Max.Z) - MathF.Max(sourceBounds.Min.Z, otherBounds.Min.Z)
    );

    if (overlap.X <= 0 || overlap.Y <= 0 || overlap.Z <= 0) return GetDominantDirection(delta);

    if (overlap.X <= overlap.Y && overlap.X <= overlap.Z) return delta.X >= 0 ? Direction.Right : Direction.Left;
    if (overlap.Y <= overlap.Z) return delta.Y >= 0 ? Direction.Top : Direction.Bottom;
    return delta.Z >= 0 ? Direction.Front : Direction.Back;
  }

  static Direction GetDominantDirection(Vector3 delta)
  {
    var absolute = new Vector3(MathF.Abs(delta.X), MathF.Abs(delta.Y), MathF.Abs(delta.Z));

    if (absolute.X >= absolute.Y && absolute.X >= absolute.Z) return delta.X >= 0 ? Direction.Right : Direction.Left;
    if (absolute.Y >= absolute.Z) return delta.Y >= 0 ? Direction.Top : Direction.Bottom;
    return delta.Z >= 0 ? Direction.Front : Direction.Back;
  }

  static float GetAxis(Vector3 value, Axis axis) => axis switch
  {
    Axis.X => value.X,
    Axis.Y => value.Y,
    _ => value.Z,
  };

  static Vector3 SetAxis(Vector3 value, Axis axis, float axisValue) => axis switch
  {
    Axis.X => new Vector3(axisValue, value.Y, value.Z),
    Axis.Y => new Vector3(value.X, axisValue, value.Z),
    _ => new Vector3(value.X, value.Y, axisValue),
  };

  enum Axis { X, Y, Z }
}
