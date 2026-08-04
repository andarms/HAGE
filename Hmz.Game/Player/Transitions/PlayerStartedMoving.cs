using Hmz.Core.States;
using Hmz.Game.Player.States;

namespace Hmz.Game.Player.Transitions;

public sealed class PlayerStartedMoving : Transition<PlayerContext, PlayerWalkingState>
{
  public override bool ShouldTransition(PlayerContext context)
  {
    return context.InputEnabled && context.IsMoving;
  }
}
