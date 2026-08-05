namespace Hmz.Core.Tilemap;

public static class TileSizeExtensions
{
  public static (int Width, int Depth) Dimensions(this TileSize size)
  {
    return size switch
    {
      TileSize.OneByOne => (1, 1),
      TileSize.OneByTwo => (1, 2),
      TileSize.OneByThree => (1, 3),
      TileSize.TwoByOne => (2, 1),
      TileSize.TwoByTwo => (2, 2),
      TileSize.TwoByThree => (2, 3),
      TileSize.ThreeByOne => (3, 1),
      TileSize.ThreeByTwo => (3, 2),
      TileSize.ThreeByThree => (3, 3),
      _ => throw new ArgumentOutOfRangeException(nameof(size), size, null),
    };
  }
}
