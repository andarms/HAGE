using Hmz.Core.GOM;

namespace Hmz.Core.Collisions;

public readonly record struct Collision(GameObject Self, GameObject Other, Direction Side);
