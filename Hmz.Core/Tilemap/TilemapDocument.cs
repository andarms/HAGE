using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Hmz.Core.Tilemap;

public class TilemapDocument
{
  public const string FormatName = "tilemap";
  public const int CurrentVersion = 1;

  static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    Converters = { new Vector3JsonConverter() },
  };

  public required string Format { get; set; }
  public required int Version { get; set; }
  public string? Name { get; set; }
  public required TilemapGrid Grid { get; set; }
  public required List<string> Palette { get; set; }
  public required List<TilemapLayer> Layers { get; set; }
  public List<TilemapObjectRecord> Objects { get; set; } = [];

  public static TilemapDocument Load(string path)
  {
    return Parse(File.ReadAllText(path));
  }

  public static TilemapDocument Parse(string json)
  {
    TilemapDocument document = JsonSerializer.Deserialize<TilemapDocument>(json, JsonOptions)
      ?? throw new InvalidDataException("Tilemap file is empty or not valid JSON.");
    document.Validate();
    return document;
  }

  public void Save(string path)
  {
    string json = JsonSerializer.Serialize(this, JsonOptions);
    string tempPath = path + ".tmp";
    File.WriteAllText(tempPath, json);
    File.Move(tempPath, path, overwrite: true);
  }

  public void Compact()
  {
    List<string> usedKeys = [];
    Dictionary<int, int> remap = [];

    for (int oldIndex = 0; oldIndex < Palette.Count; oldIndex++)
    {
      bool inUse = Layers.Any(layer => layer.Tiles.Any(tile => tile.T == oldIndex));
      if (!inUse)
      {
        continue;
      }

      remap[oldIndex] = usedKeys.Count;
      usedKeys.Add(Palette[oldIndex]);
    }

    foreach (TilemapLayer layer in Layers)
    {
      foreach (TileRecord tile in layer.Tiles)
      {
        tile.T = remap[tile.T];
      }
    }

    Palette = usedKeys;
  }

  void Validate()
  {
    if (Format != FormatName)
    {
      throw new InvalidDataException($"Unsupported tilemap format '{Format}'; expected '{FormatName}'.");
    }

    if (Version > CurrentVersion)
    {
      throw new InvalidDataException($"Tilemap version {Version} is newer than the supported version {CurrentVersion}.");
    }

    if (Version < CurrentVersion)
    {
      throw new InvalidDataException($"Tilemap version {Version} predates the supported version {CurrentVersion}, and no migration path is implemented.");
    }
  }
}

public class TilemapGrid
{
  public required int Width { get; set; }
  public required int Depth { get; set; }
  public required float CellSize { get; set; }
  public required float LayerHeight { get; set; }
  public required int LayerCount { get; set; }
}

public class TilemapLayer
{
  public required int Index { get; set; }
  public string? Role { get; set; }
  public List<TileRecord> Tiles { get; set; } = [];
}

public class TileRecord
{
  public required int T { get; set; }
  public required int X { get; set; }
  public required int Z { get; set; }
  public required int R { get; set; }
  public JsonObject? Properties { get; set; }
}

public class TilemapObjectRecord
{
  public required string Type { get; set; }
  public required Vector3 Pos { get; set; }
  public required Vector3 Rot { get; set; }
  public Vector3? Scale { get; set; }
  public JsonObject? Properties { get; set; }
}
