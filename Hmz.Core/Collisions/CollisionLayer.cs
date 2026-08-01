namespace Hmz.Core.Collisions;

[Flags]
public enum CollisionLayer
{
  None = 0,
  Player = 1 << 0,
  Enemy = 1 << 1,
  NPC = 1 << 2,
  Environment = 1 << 3,
  All = Player | Enemy | NPC | Environment,
}
