using System.Numerics;
using Hmz.Core.GOM;

namespace Hmz.Core.Tilemap;

public static class TilemapLoader
{
  public static void Load(string path, GameObjectCollection target)
  {
    Instantiate(TilemapDocument.Load(path), target);
  }

  public static void Instantiate(TilemapDocument document, GameObjectCollection target)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(target);

    TypeRegistry<Tile3D> tiles = Engine.GameObjectRegistry.Tiles;
    TypeRegistry<GameObject> objects = Engine.GameObjectRegistry.Objects;

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
    foreach (TileRecord record in layer.Tiles)
    {
      target.Add(InstantiateTile(document, layer.Index, record, tiles));
    }
  }

  public static Tile3D InstantiateTile(TilemapDocument document, int layerIndex, TileRecord record) =>
    InstantiateTile(document, layerIndex, record, Engine.GameObjectRegistry.Tiles);

  static Tile3D InstantiateTile(TilemapDocument document, int layerIndex, TileRecord record, TypeRegistry<Tile3D> tiles)
  {
    if (record.T < 0 || record.T >= document.Palette.Count)
    {
      throw new InvalidDataException(
        $"Layer {layerIndex}: tile at ({record.X}, {record.Z}) references palette index {record.T}, " +
        $"but the palette only has {document.Palette.Count} entries.");
    }

    string key = document.Palette[record.T];
    if (!tiles.Contains(key))
    {
      throw new KeyNotFoundException(
        $"Unknown tile type key '{key}' (palette index {record.T}), used by a tile on layer {layerIndex} " +
        $"at ({record.X}, {record.Z}). Searched the tile registry.");
    }

    Tile3D tile = tiles.Create(key, record.Properties);

    float y = layerIndex * document.Grid.LayerHeight;
    (float centerX, float centerZ) = TileOccupancy.GetCenter(tile.Size, record.X, record.Z);
    tile.Transform.Position = new Vector3(centerX * document.Grid.CellSize, y, centerZ * document.Grid.CellSize);
    tile.RotateTile(record.R * MathF.PI / 2f);

    return tile;
  }

  public static GameObject InstantiateObject(TilemapObjectRecord record) =>
    InstantiateObject(record, Engine.GameObjectRegistry.Objects);

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
