using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class Movement(PlayerContext context) : Component
{
  public float Speed { get; set; } = 5f;
  public float RotationSpeed { get; set; } = 10f;

  Vector3 direction = Vector3.Zero;

  public override void HandleInput()
  {
    direction = Vector3.Zero;

    if (context.InputEnabled)
    {
      if (Engine.Input.IsActionPressed("move_up")) direction.Z -= 1f;
      if (Engine.Input.IsActionPressed("move_down")) direction.Z += 1f;
      if (Engine.Input.IsActionPressed("move_left")) direction.X -= 1f;
      if (Engine.Input.IsActionPressed("move_right")) direction.X += 1f;

      if (direction != Vector3.Zero) direction = Vector3.Normalize(direction);
    }

    context.IsMoving = direction != Vector3.Zero;
  }

  public override void Update(float dt)
  {
    Move(dt);
  }

  void Move(float dt)
  {
    Vector3 currentDirection = direction;

    if (currentDirection == Vector3.Zero) return;

    float targetYaw = MathF.Atan2(currentDirection.X, currentDirection.Z);
    Quaternion targetRotation = Quaternion.CreateFromYawPitchRoll(targetYaw, 0f, 0f);
    Owner.Transform.Rotation = Quaternion.Slerp(Owner.Transform.Rotation, targetRotation, MathF.Min(RotationSpeed * dt, 1f));

    Vector3 targetPosition = Owner.Transform.Position + currentDirection * Speed * dt;
    Owner.Transform.Position = Engine.Collisions.MoveAndCollide(Owner, targetPosition);
  }
}