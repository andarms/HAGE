using Hmz.Core.GOM;

namespace Hmz.Core.Collisions;

// Uniform grid over the XZ plane, cell size matched to the smallest tile so bigger objects
// just span more cells. Static objects (their bounds never change) cost one Move call ever;
// Move is a cheap no-op whenever an object's cell range hasn't changed.
public class SpatialGrid(float cellSize)
{
  readonly Dictionary<(int X, int Z), List<GameObject>> cells = [];
  readonly Dictionary<GameObject, CellRange> ranges = [];

  public void Move(GameObject obj, CollisionBox bounds)
  {
    var range = GetRange(bounds);

    if (ranges.TryGetValue(obj, out var current))
    {
      if (current == range)
      {
        return;
      }

      RemoveFromCells(obj, current);
    }

    ranges[obj] = range;
    AddToCells(obj, range);
  }

  public void Remove(GameObject obj)
  {
    if (!ranges.Remove(obj, out var range))
    {
      return;
    }

    RemoveFromCells(obj, range);
  }

  public void Query(CollisionBox area, HashSet<GameObject> results)
  {
    var range = GetRange(area);

    for (int x = range.MinX; x <= range.MaxX; x++)
    {
      for (int z = range.MinZ; z <= range.MaxZ; z++)
      {
        if (cells.TryGetValue((x, z), out var objects))
        {
          foreach (var obj in objects)
          {
            results.Add(obj);
          }
        }
      }
    }
  }

  void AddToCells(GameObject obj, CellRange range)
  {
    for (int x = range.MinX; x <= range.MaxX; x++)
    {
      for (int z = range.MinZ; z <= range.MaxZ; z++)
      {
        var key = (x, z);

        if (!cells.TryGetValue(key, out var objects))
        {
          objects = [];
          cells[key] = objects;
        }

        objects.Add(obj);
      }
    }
  }

  void RemoveFromCells(GameObject obj, CellRange range)
  {
    for (int x = range.MinX; x <= range.MaxX; x++)
    {
      for (int z = range.MinZ; z <= range.MaxZ; z++)
      {
        var key = (x, z);

        if (!cells.TryGetValue(key, out var objects))
        {
          continue;
        }

        objects.Remove(obj);

        if (objects.Count == 0)
        {
          cells.Remove(key);
        }
      }
    }
  }

  CellRange GetRange(CollisionBox bounds)
  {
    return new CellRange(
      (int)MathF.Floor(bounds.Min.X / cellSize),
      (int)MathF.Floor(bounds.Max.X / cellSize),
      (int)MathF.Floor(bounds.Min.Z / cellSize),
      (int)MathF.Floor(bounds.Max.Z / cellSize)
    );
  }

  readonly record struct CellRange(int MinX, int MaxX, int MinZ, int MaxZ);
}
