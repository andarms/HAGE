using Hmz.Core.States;
using Hmz.Game.Player.Transitions;

namespace Hmz.Game.Player.States;

public sealed class PlayerWalkingState : State<PlayerContext>
{
  public override IEnumerable<Transition<PlayerContext>> Transitions { get; } =
  [
    new PlayerAttackRequested(),
    new PlayerStopped(),
  ];

  public override void Enter(PlayerContext context)
  {
    context.AnimationName = "walk";
    context.AnimationLoop = true;
  }
}
