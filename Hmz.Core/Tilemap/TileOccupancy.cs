namespace Hmz.Core.Tilemap;

public static class TileOccupancy
{
  public static (float X, float Z) GetCenter(TileSize size, int x, int z)
  {
    (int width, int depth) = size.Dimensions();
    return (x + width / 2f, z + depth / 2f);
  }

  public static IEnumerable<(int X, int Z)> GetOccupiedCells(TileSize size, int x, int z, int r)
  {
    (int width, int depth) = size.Dimensions();
    (float centerX, float centerZ) = GetCenter(size, x, z);

    bool even = r % 2 == 0;
    int extentX = even ? width : depth;
    int extentZ = even ? depth : width;

    int minX = RoundHalfAwayFromZero(centerX - extentX / 2f);
    int minZ = RoundHalfAwayFromZero(centerZ - extentZ / 2f);

    for (int cx = minX; cx < minX + extentX; cx++)
    {
      for (int cz = minZ; cz < minZ + extentZ; cz++)
      {
        yield return (cx, cz);
      }
    }
  }

  // Arbitrary choice; the editor must round the same way or the two disagree on occupied cells.
  static int RoundHalfAwayFromZero(float value)
  {
    return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
  }
}
