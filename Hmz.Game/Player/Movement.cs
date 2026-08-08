using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;
using Hmz.Core.Input;

namespace Hmz.Game.Player;

public class Movement(PlayerContext context) : Component
{
  const float StickDeadzone = 0.25f;

  public float Speed { get; set; } = 5f;
  public float RotationSpeed { get; set; } = 10f;
  public float GravityAcceleration { get; set; } = 50f;
  public float MaxFallSpeed { get; set; } = 100f;

  Vector3 direction = Vector3.Zero;
  float fallSpeed = 0f;

  public override void HandleInput()
  {
    direction = Vector3.Zero;

    if (context.InputEnabled)
    {
      if (Engine.Input.IsActionPressed("move_up")) direction.Z -= 1f;
      if (Engine.Input.IsActionPressed("move_down")) direction.Z += 1f;
      if (Engine.Input.IsActionPressed("move_left")) direction.X -= 1f;
      if (Engine.Input.IsActionPressed("move_right")) direction.X += 1f;

      float stickX = Engine.Input.GetGamepadAxis(GamepadAxis.LeftStickX);
      float stickY = Engine.Input.GetGamepadAxis(GamepadAxis.LeftStickY);
      if (MathF.Abs(stickX) > StickDeadzone)
      {
        direction.X += stickX;
      }
      if (MathF.Abs(stickY) > StickDeadzone)
      {
        direction.Z += stickY;
      }

      if (direction != Vector3.Zero)
      {
        direction = Vector3.Transform(direction, Quaternion.CreateFromYawPitchRoll(context.CameraYaw, 0f, 0f));
        direction = Vector3.Normalize(direction);
      }
    }

    context.IsMoving = direction != Vector3.Zero;
  }

  public override void Update(float dt)
  {
    Move(dt);
    Fall(dt);
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

  void Fall(float dt)
  {
    if (IsOnSlope())
    {
      Owner.Transform.Position = Engine.Collisions.SnapToGround(Owner);
      fallSpeed = 0f;
      return;
    }

    if (Engine.Collisions.IsGrounded(Owner))
    {
      fallSpeed = 0f;
      return;
    }

    fallSpeed = MathF.Min(fallSpeed + GravityAcceleration * dt, MaxFallSpeed);
    Owner.Transform.Position = Engine.Collisions.ApplyGravity(Owner, fallSpeed * dt);
  }

  bool IsOnSlope()
  {
    float? groundHeight = Engine.Collisions.GetGroundHeight(Owner);

    if (groundHeight == null)
    {
      return false;
    }

    float heightDifference = MathF.Abs(Owner.Transform.Position.Y - groundHeight.Value);
    return heightDifference <= Engine.Collisions.StepHeight;
  }
}