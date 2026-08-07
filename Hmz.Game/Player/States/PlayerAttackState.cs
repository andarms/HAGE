using Hmz.Core.States;
using Hmz.Game.Player.Transitions;

namespace Hmz.Game.Player.States;

public sealed class PlayerAttackState : State<PlayerContext>
{
  public override IEnumerable<Transition<PlayerContext>> Transitions { get; } =
  [
    new PlayerAttackFinished(),
  ];

  public override void Enter(PlayerContext context)
  {
    context.AnimationName = "attack_right";
    context.AnimationLoop = false;
    context.Movement?.Enabled = false;
  }

  public override void Exit(PlayerContext context)
  {
    context.Movement?.Enabled = true;
  }
}
