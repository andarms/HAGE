using Hmz.Core.States;
using Hmz.Game.Player.States;

namespace Hmz.Game.Player.Transitions;

public sealed class PlayerAttackFinished : Transition<PlayerContext, PlayerIdleState>
{
  public override bool ShouldTransition(PlayerContext context)
  {
    return context.AnimationFinished;
  }
}
