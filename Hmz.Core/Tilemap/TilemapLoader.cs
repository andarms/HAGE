using System.Numerics;
using Hmz.Core.GOM;

namespace Hmz.Core.Tilemap;

public static class TilemapLoader
{
  public static void Load(string path, TypeRegistry<Tile3D> tiles, TypeRegistry<GameObject> objects, GameObjectCollection target)
  {
    Instantiate(TilemapDocument.Load(path), tiles, objects, target);
  }

  public static void Instantiate(TilemapDocument document, TypeRegistry<Tile3D> tiles, TypeRegistry<GameObject> objects, GameObjectCollection target)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(objects);
    ArgumentNullException.ThrowIfNull(target);

    foreach (TilemapLayer layer in document.Layers)
    {
      InstantiateLayer(document, layer, tiles, target);
    }

    foreach (TilemapObjectRecord record in document.Objects)
    {
      target.Add(InstantiateObject(record, objects));
    }
  }

  static void InstantiateLayer(TilemapDocument document, TilemapLayer layer, TypeRegistry<Tile3D> tiles, GameObjectCollection target)
  {
    float y = layer.Index * document.Grid.LayerHeight;

    foreach (TileRecord record in layer.Tiles)
    {
      if (record.T < 0 || record.T >= document.Palette.Count)
      {
        throw new InvalidDataException(
          $"Layer {layer.Index}: tile at ({record.X}, {record.Z}) references palette index {record.T}, " +
          $"but the palette only has {document.Palette.Count} entries.");
      }

      string key = document.Palette[record.T];
      if (!tiles.Contains(key))
      {
        throw new KeyNotFoundException(
          $"Unknown tile type key '{key}' (palette index {record.T}), used by a tile on layer {layer.Index} " +
          $"at ({record.X}, {record.Z}). Searched the tile registry.");
      }

      Tile3D tile = tiles.Create(key, record.Properties);

      (float centerX, float centerZ) = TileOccupancy.GetCenter(tile.Size, record.X, record.Z);
      tile.Transform.Position = new Vector3(centerX * document.Grid.CellSize, y, centerZ * document.Grid.CellSize);
      tile.RotateTile(record.R * MathF.PI / 2f);

      target.Add(tile);
    }
  }

  static GameObject InstantiateObject(TilemapObjectRecord record, TypeRegistry<GameObject> objects)
  {
    if (!objects.Contains(record.Type))
    {
      throw new KeyNotFoundException(
        $"Unknown object type key '{record.Type}', used by an object record. Searched the object registry.");
    }

    GameObject instance = objects.Create(record.Type, record.Properties);
    instance.Transform.Position = record.Pos;
    instance.Transform.EulerAngles = record.Rot * (MathF.PI / 180f);
    instance.Transform.Scale = record.Scale ?? Vector3.One;

    return instance;
  }
}
