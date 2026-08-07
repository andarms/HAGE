using Hmz.Core.GOM;

namespace Hmz.Core.Tilemap;

public class GameObjectRegistry
{
  public TypeRegistry<Tile3D> Tiles { get; } = new();
  public TypeRegistry<GameObject> Objects { get; } = new();
}
