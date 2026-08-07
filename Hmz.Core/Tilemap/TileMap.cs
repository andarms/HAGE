using Hmz.Core.GOM;

namespace Hmz.Core.Tilemap;

public class TileMap : GameObject
{
  public static TileMap FromDocument(TilemapDocument document)
  {
    TileMap map = new();
    TilemapLoader.Instantiate(document, map.Children);
    return map;
  }
}
