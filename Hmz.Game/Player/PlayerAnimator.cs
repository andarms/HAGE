using Hmz.Core._3D;
using Hmz.Core.GOM;

namespace Hmz.Game.Player;

public sealed class PlayerAnimator(PlayerContext context) : Component
{
  ModelRenderer renderer = null!;
  string? current;

  public override void Initialize()
  {
    renderer = Owner.Components.Require<ModelRenderer>();
    renderer.AnimationFinished += OnAnimationFinished;
  }

  public override void Update(float dt)
  {
    if (context.AnimationName == current) return;

    current = context.AnimationName;
    if (current != null)
    {
      context.AnimationFinished = false;
      renderer.Play(current, context.AnimationLoop);
    }
  }

  void OnAnimationFinished(string animationName)
  {
    context.AnimationFinished = true;
  }
}
