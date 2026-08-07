namespace Hmz.Game.Player;

public sealed class PlayerContext
{
  public bool InputEnabled { get; set; }

  public bool IsMoving { get; set; }

  public bool AttackPressed { get; set; }

  public string? AnimationName { get; set; }

  public bool AnimationLoop { get; set; } = true;

  public bool AnimationFinished { get; set; }

  public float CameraYaw { get; set; }

  public Movement? Movement { get; set; }
}