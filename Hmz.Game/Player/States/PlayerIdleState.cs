using Hmz.Core.States;
using Hmz.Game.Player.Transitions;

namespace Hmz.Game.Player.States;

public sealed class PlayerIdleState : State<PlayerContext>
{
  public override IEnumerable<Transition<PlayerContext>> Transitions { get; } =
  [
    new PlayerStartedMoving(),
  ];

  public override void Enter(PlayerContext context)
  {
    context.AnimationName = "idle";
  }
}
