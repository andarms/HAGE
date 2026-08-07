using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class CameraController(PlayerContext context) : Component
{
  public float Distance { get; set; } = 13f;
  public float MinDistance { get; set; } = 10f;
  public float MaxDistance { get; set; } = 18f;
  public float ZoomSpeed { get; set; } = 10f;

  public float Pitch { get; set; } = MathF.Atan2(8f, 10f);
  public float RotationStep { get; set; } = MathF.PI / 4f;
  public float RotationSpeed { get; set; } = 10f;
  public float FollowSpeed { get; set; } = 8f;

  public float Yaw { get; private set; }

  float targetYaw;
  float zoomInput;

  public override void Initialize()
  {
    context.CameraYaw = Yaw;

    Vector3 offset = ComputeOffset();
    Engine.MainCamera.Position = Owner.Transform.Position + offset;
    Engine.MainCamera.Target = Owner.Transform.Position;
  }

  public override void HandleInput()
  {
    zoomInput = 0f;

    if (!context.InputEnabled)
    {
      return;
    }

    if (Engine.Input.IsActionJustPressed("camera_left"))
    {
      targetYaw -= RotationStep;
    }
    if (Engine.Input.IsActionJustPressed("camera_right"))
    {
      targetYaw += RotationStep;
    }
    if (Engine.Input.IsActionPressed("camera_up"))
    {
      zoomInput -= 1f;
    }
    if (Engine.Input.IsActionPressed("camera_down"))
    {
      zoomInput += 1f;
    }
  }

  public override void Update(float dt)
  {
    Yaw = float.Lerp(Yaw, targetYaw, MathF.Min(RotationSpeed * dt, 1f));
    Distance = Math.Clamp(Distance + zoomInput * ZoomSpeed * dt, MinDistance, MaxDistance);
    context.CameraYaw = Yaw;

    Engine.MainCamera.Follow(Owner.Transform.Position, ComputeOffset(), FollowSpeed, dt);
  }

  Vector3 ComputeOffset()
  {
    float horizontalDistance = Distance * MathF.Cos(Pitch);
    float height = Distance * MathF.Sin(Pitch);
    Vector3 direction = new(MathF.Sin(Yaw), 0f, MathF.Cos(Yaw));

    return direction * horizontalDistance + Vector3.UnitY * height;
  }
}
