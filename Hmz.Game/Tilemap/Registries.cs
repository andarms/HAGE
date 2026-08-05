using Hmz.Core.GOM;
using Hmz.Core.Tilemap;

namespace Hmz.Game.Tilemap;

public static class Registries
{
  public static readonly TypeRegistry<Tile3D> Tiles = new();
  public static readonly TypeRegistry<GameObject> Objects = new();

  static bool registered;

  public static void RegisterAll()
  {
    if (registered)
    {
      return;
    }
    registered = true;

    Tiles.Register("Wall", props => new Wall());

    Objects.Register("Tree", props => new Tree());
    Objects.Register("Crate", props => new Crate());
  }
}
