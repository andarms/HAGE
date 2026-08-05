using Hmz.Core.GOM;

namespace Hmz.Core.Tilemap;

public class TileMap : GameObject
{
  public static TileMap FromDocument(TilemapDocument document, TypeRegistry<Tile3D> tiles, TypeRegistry<GameObject> objects)
  {
    TileMap map = new();
    TilemapLoader.Instantiate(document, tiles, objects, map.Children);
    return map;
  }
}
