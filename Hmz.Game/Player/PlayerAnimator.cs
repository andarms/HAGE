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
  }

  public override void Update(float dt)
  {
    if (context.AnimationName == current) return;

    current = context.AnimationName;
    if (current != null) renderer.Play(current);
  }
}
