using System.Numerics;
using Hmz.Core;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public class Movement : Component
{
  public float Speed { get; set; } = 5f;

  public override void Update(float dt)
  {
    Vector3 direction = Vector3.Zero;

    if (Engine.Input.IsActionPressed("move_up")) direction.Z -= 1f;
    if (Engine.Input.IsActionPressed("move_down")) direction.Z += 1f;
    if (Engine.Input.IsActionPressed("move_left")) direction.X -= 1f;
    if (Engine.Input.IsActionPressed("move_right")) direction.X += 1f;

    if (direction == Vector3.Zero) return;

    direction = Vector3.Normalize(direction);
    Vector3 targetPosition = Owner.Transform.Position + direction * Speed * dt;
    Owner.Transform.Position = Engine.Collisions.MoveAndCollide(Owner, targetPosition);
  }
}