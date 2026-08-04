using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class Movement : Component
{
  public float Speed { get; set; } = 5f;
  public float RotationSpeed { get; set; } = 10f;

  Vector3 direction = Vector3.Zero;

  // Only runs while this object's scene is the active one (see SceneManager.Update), so
  // a stacked scene (e.g. behind the editor) stops steering the player.
  public override void HandleInput()
  {
    direction = Vector3.Zero;

    if (Engine.Input.IsActionPressed("move_up")) direction.Z -= 1f;
    if (Engine.Input.IsActionPressed("move_down")) direction.Z += 1f;
    if (Engine.Input.IsActionPressed("move_left")) direction.X -= 1f;
    if (Engine.Input.IsActionPressed("move_right")) direction.X += 1f;

    if (direction != Vector3.Zero) direction = Vector3.Normalize(direction);
  }

  public override void Update(float dt)
  {
    // Runs even while stacked; consume the last direction once so a scene that stops
    // receiving input (paused/stacked) settles to idle instead of drifting forever.
    Vector3 currentDirection = direction;
    direction = Vector3.Zero;

    if (currentDirection == Vector3.Zero) return;

    // Model's neutral pose faces +Z, so yaw = angle from +Z to the movement direction.
    float targetYaw = MathF.Atan2(currentDirection.X, currentDirection.Z);
    Quaternion targetRotation = Quaternion.CreateFromYawPitchRoll(targetYaw, 0f, 0f);
    Owner.Transform.Rotation = Quaternion.Slerp(Owner.Transform.Rotation, targetRotation, MathF.Min(RotationSpeed * dt, 1f));

    Vector3 targetPosition = Owner.Transform.Position + currentDirection * Speed * dt;
    Owner.Transform.Position = Engine.Collisions.MoveAndCollide(Owner, targetPosition);
  }
}