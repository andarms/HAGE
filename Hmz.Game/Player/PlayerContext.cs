namespace Hmz.Game.Player;

public sealed class PlayerContext
{
  public bool InputEnabled { get; set; }

  public bool IsMoving { get; set; }

  public string? AnimationName { get; set; }
}