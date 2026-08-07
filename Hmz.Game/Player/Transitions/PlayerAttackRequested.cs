using Hmz.Core.States;
using Hmz.Game.Player.States;

namespace Hmz.Game.Player.Transitions;

public sealed class PlayerAttackRequested : Transition<PlayerContext, PlayerAttackState>
{
  public override bool ShouldTransition(PlayerContext context)
  {
    return context.InputEnabled && context.AttackPressed;
  }
}
