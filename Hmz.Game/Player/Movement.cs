using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class Movement : Component
{
  public float Speed { get; set; } = 5f;
  public float RotationSpeed { get; set; } = 10f;

  public override void Update(float dt)
  {
    Vector3 direction = Vector3.Zero;

    if (Engine.Input.IsActionPressed("move_up")) direction.Z -= 1f;
    if (Engine.Input.IsActionPressed("move_down")) direction.Z += 1f;
    if (Engine.Input.IsActionPressed("move_left")) direction.X -= 1f;
    if (Engine.Input.IsActionPressed("move_right")) direction.X += 1f;

    if (direction == Vector3.Zero) return;

    direction = Vector3.Normalize(direction);

    // Model's neutral pose faces +Z, so yaw = angle from +Z to the movement direction.
    float targetYaw = MathF.Atan2(direction.X, direction.Z);
    float currentYaw = Owner.Transform.Rotation.Y;
    float newYaw = LerpAngle(currentYaw, targetYaw, MathF.Min(RotationSpeed * dt, 1f));
    Owner.Transform.Rotation = new Vector3(0f, newYaw, 0f);

    Vector3 targetPosition = Owner.Transform.Position + direction * Speed * dt;
    Owner.Transform.Position = Engine.Collisions.MoveAndCollide(Owner, targetPosition);
  }

  // Lerps by the shortest angular path, so crossing the -pi/pi wraparound doesn't spin the long way.
  static float LerpAngle(float from, float to, float t)
  {
    float delta = ((to - from + MathF.PI) % MathF.Tau + MathF.Tau) % MathF.Tau - MathF.PI;
    return from + delta * t;
  }
}