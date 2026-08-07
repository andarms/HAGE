using System.Numerics;
using Hmz.Core;
using Hmz.Core._3D.Geometry;
using Hmz.Core.GOM;
using Hmz.Core.Input;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Tilemap;

namespace Hmz.Editor;

public enum EditorMode { Tile, Object }

// The placement cursor: what type is selected, where it is, and how it moves. Position is
// tracked as continuous world-space X/Z rather than a cell index, so switching modes never
// resets or jumps the ghost - only the step size and how the position is used change.
public sealed class EditorGhost
{
  const float MoveRepeatDelay = 0.25f;
  const float MoveRepeatInterval = 0.12f;
  const float ObjectMoveStep = 1f;
  const float ObjectMoveStepFine = 0.1f;

  public EditorMode Mode { get; private set; } = EditorMode.Tile;
  public int CurrentLayer { get; set; }
  public int RotationStep { get; private set; }

  float x;
  float z;

  List<string> tileKeys = [];
  List<string> objectKeys = [];
  int tileTypeIndex;
  int objectTypeIndex;

  readonly Dictionary<Key, float> moveRepeatTimers = new()
  {
    [Key.UpArrow] = 0f,
    [Key.DownArrow] = 0f,
    [Key.LeftArrow] = 0f,
    [Key.RightArrow] = 0f,
  };

  public string? SelectedTileKey => tileKeys.Count > 0 ? tileKeys[tileTypeIndex] : null;
  public string? SelectedObjectKey => objectKeys.Count > 0 ? objectKeys[objectTypeIndex] : null;

  public void Initialize()
  {
    tileKeys = [.. Engine.GameObjectRegistry.Tiles.Keys];
    objectKeys = [.. Engine.GameObjectRegistry.Objects.Keys];
  }

  public void HandleModeAndTypeSelection()
  {
    if (Engine.Input.IsKeyJustPressed(Key.Num1)) Mode = EditorMode.Tile;
    if (Engine.Input.IsKeyJustPressed(Key.Num2)) Mode = EditorMode.Object;

    if (!Engine.Input.IsKeyJustPressed(Key.Tab)) return;

    if (Mode == EditorMode.Tile && tileKeys.Count > 0)
    {
      tileTypeIndex = (tileTypeIndex + 1) % tileKeys.Count;
    }
    else if (Mode == EditorMode.Object && objectKeys.Count > 0)
    {
      objectTypeIndex = (objectTypeIndex + 1) % objectKeys.Count;
    }
  }

  public void HandleRotation()
  {
    if (Engine.Input.IsKeyJustPressed(Key.R))
    {
      RotationStep = (RotationStep + 1) % 4;
    }
  }

  public void HandleMovement(TilemapGrid grid, float dt)
  {
    TryStepMove(Key.UpArrow, 0, -1, grid, dt);
    TryStepMove(Key.DownArrow, 0, 1, grid, dt);
    TryStepMove(Key.LeftArrow, -1, 0, grid, dt);
    TryStepMove(Key.RightArrow, 1, 0, grid, dt);
  }

  void TryStepMove(Key key, int dx, int dz, TilemapGrid grid, float dt)
  {
    if (!Engine.Input.IsKeyPressed(key))
    {
      moveRepeatTimers[key] = 0f;
      return;
    }

    if (Engine.Input.IsKeyJustPressed(key))
    {
      Step(dx, dz, grid);
      moveRepeatTimers[key] = MoveRepeatDelay;
      return;
    }

    moveRepeatTimers[key] -= dt;
    if (moveRepeatTimers[key] > 0f) return;

    Step(dx, dz, grid);
    moveRepeatTimers[key] = MoveRepeatInterval;
  }

  void Step(int dx, int dz, TilemapGrid grid)
  {
    bool fine = Engine.Input.IsKeyPressed(Key.LeftShift) || Engine.Input.IsKeyPressed(Key.RightShift);
    float step = Mode == EditorMode.Tile ? grid.CellSize : (fine ? ObjectMoveStepFine : ObjectMoveStep);
    x = Math.Clamp(x + dx * step, 0f, grid.Width * grid.CellSize);
    z = Math.Clamp(z + dz * step, 0f, grid.Depth * grid.CellSize);
  }

  public int CellX(TilemapGrid grid) => Math.Clamp((int)MathF.Round(x / grid.CellSize), 0, grid.Width - 1);
  public int CellZ(TilemapGrid grid) => Math.Clamp((int)MathF.Round(z / grid.CellSize), 0, grid.Depth - 1);

  public Vector3 WorldPosition(TilemapGrid grid)
  {
    float y = CurrentLayer * grid.LayerHeight;
    if (Mode == EditorMode.Tile)
    {
      return new Vector3((CellX(grid) + 0.5f) * grid.CellSize, y, (CellZ(grid) + 0.5f) * grid.CellSize);
    }
    return new Vector3(SnapTenth(x), y, SnapTenth(z));
  }

  static float SnapTenth(float value) => MathF.Round(value * 10f, MidpointRounding.AwayFromZero) / 10f;


  public void DrawPreview(TilemapGrid grid)
  {
    if (Mode == EditorMode.Tile) DrawTilePreview(grid);
    else DrawObjectPreview(grid);
  }

  void DrawTilePreview(TilemapGrid grid)
  {
    if (SelectedTileKey is not { } key) return;

    Tile3D ghost = Engine.GameObjectRegistry.Tiles.Create(key, null);
    List<(int X, int Z)> cells = [.. TileOccupancy.GetOccupiedCells(ghost.Size, CellX(grid), CellZ(grid), RotationStep)];
    if (cells.Count == 0) return;

    int minX = cells.Min(c => c.X);
    int maxX = cells.Max(c => c.X);
    int minZ = cells.Min(c => c.Z);
    int maxZ = cells.Max(c => c.Z);

    float width = (maxX - minX + 1) * grid.CellSize;
    float depth = (maxZ - minZ + 1) * grid.CellSize;
    float height = grid.LayerHeight;
    float centerX = (minX + (maxX - minX + 1) / 2f) * grid.CellSize;
    float centerZ = (minZ + (maxZ - minZ + 1) / 2f) * grid.CellSize;
    float y = CurrentLayer * grid.LayerHeight + height / 2f;

    Cube cube = new() { Size = new Vector3(width, height, depth) };
    Matrix4x4 worldMatrix = Matrix4x4.CreateTranslation(centerX, y, centerZ);
    Engine.Graphics.DrawCube(cube, worldMatrix, new CubeStyle { Wireframe = true, Color = Color.Cyan });
  }

  void DrawObjectPreview(TilemapGrid grid)
  {
    if (SelectedObjectKey is not { } key) return;

    GameObject ghost = Engine.GameObjectRegistry.Objects.Create(key, null);
    Vector3 size = ghost.Collider?.Size ?? Vector3.One;
    Vector3 offset = ghost.Collider?.Offset ?? Vector3.Zero;

    Cube cube = new() { Size = size };
    Matrix4x4 worldMatrix = Matrix4x4.CreateTranslation(WorldPosition(grid) + offset);
    Engine.Graphics.DrawCube(cube, worldMatrix, new CubeStyle { Wireframe = true, Color = Color.Cyan });
  }
}
