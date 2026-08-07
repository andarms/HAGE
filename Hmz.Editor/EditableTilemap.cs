using System.Numerics;
using Hmz.Core.GOM;
using Hmz.Core.Tilemap;

namespace Hmz.Editor;

// Owns the TilemapDocument and its live TileMap, and is the only thing that mutates either.
// Incremental Place/Erase methods edit the live TileMap directly, rather than rebuilding it -
// see docs/specs/tilemap-editor.md, Section 2.
public sealed class EditableTilemap
{
  public TilemapDocument Document { get; private set; } = null!;
  public TileMap? Map { get; private set; }
  public TilemapGrid Grid => Document.Grid;

  readonly Dictionary<(int Layer, int X, int Z), Tile3D> tileInstances = [];
  readonly Dictionary<TilemapObjectRecord, GameObject> objectInstances = [];

  public void Load(string path)
  {
    Document = TilemapDocument.Load(path);
    Rebuild();
  }

  void Rebuild()
  {
    tileInstances.Clear();
    objectInstances.Clear();

    Map = new TileMap();
    foreach (TilemapLayer layer in Document.Layers)
    {
      foreach (TileRecord record in layer.Tiles)
      {
        Tile3D tile = TilemapLoader.InstantiateTile(Document, layer.Index, record);
        Map.Children.Add(tile);
        foreach ((int x, int z) in TileOccupancy.GetOccupiedCells(tile.Size, record.X, record.Z, record.R))
        {
          tileInstances[(layer.Index, x, z)] = tile;
        }
      }
    }
    foreach (TilemapObjectRecord record in Document.Objects)
    {
      GameObject instance = TilemapLoader.InstantiateObject(record);
      Map.Children.Add(instance);
      objectInstances[record] = instance;
    }
  }

  public void PlaceTile(int layerIndex, int x, int z, string typeKey, int rotationStep)
  {
    if (Map == null) return;

    EraseTile(layerIndex, x, z);

    int paletteIndex = Document.Palette.IndexOf(typeKey);
    if (paletteIndex < 0)
    {
      Document.Palette.Add(typeKey);
      paletteIndex = Document.Palette.Count - 1;
    }

    TilemapLayer? layer = Document.Layers.FirstOrDefault(l => l.Index == layerIndex);
    if (layer == null)
    {
      layer = new TilemapLayer { Index = layerIndex, Tiles = [] };
      Document.Layers.Add(layer);
    }

    TileRecord record = new() { T = paletteIndex, X = x, Z = z, R = rotationStep };
    layer.Tiles.Add(record);

    Tile3D tile = TilemapLoader.InstantiateTile(Document, layerIndex, record);
    Map.Children.Add(tile);
    foreach ((int cx, int cz) in TileOccupancy.GetOccupiedCells(tile.Size, x, z, rotationStep))
    {
      tileInstances[(layerIndex, cx, cz)] = tile;
    }
  }

  public void EraseTile(int layerIndex, int x, int z)
  {
    if (Map == null) return;
    if (!tileInstances.TryGetValue((layerIndex, x, z), out Tile3D? tile)) return;

    Map.Children.Remove(tile);

    foreach (var cellKey in tileInstances.Where(kv => ReferenceEquals(kv.Value, tile)).Select(kv => kv.Key).ToList())
    {
      tileInstances.Remove(cellKey);
    }

    TilemapLayer? layer = Document.Layers.FirstOrDefault(l => l.Index == layerIndex);
    layer?.Tiles.RemoveAll(t => TileOccupancy.GetOccupiedCells(tile.Size, t.X, t.Z, t.R).Contains((x, z)));
  }

  public void PlaceObject(string typeKey, Vector3 pos, Vector3 rot)
  {
    if (Map == null) return;

    EraseObject(pos);

    TilemapObjectRecord record = new() { Type = typeKey, Pos = pos, Rot = rot };
    Document.Objects.Add(record);

    GameObject instance = TilemapLoader.InstantiateObject(record);
    Map.Children.Add(instance);
    objectInstances[record] = instance;
  }

  public void EraseObject(Vector3 pos)
  {
    if (Map == null) return;

    TilemapObjectRecord? match = Document.Objects.FirstOrDefault(o =>
      MathF.Abs(o.Pos.X - pos.X) < 0.01f &&
      MathF.Abs(o.Pos.Y - pos.Y) < 0.01f &&
      MathF.Abs(o.Pos.Z - pos.Z) < 0.01f);

    if (match == null) return;

    if (objectInstances.Remove(match, out GameObject? instance))
    {
      Map.Children.Remove(instance);
    }
    Document.Objects.Remove(match);
  }

  public void Save(string path)
  {
    Document.Compact();
    Document.Save(path);
  }
}
