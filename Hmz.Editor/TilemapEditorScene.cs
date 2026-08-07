using System.Numerics;
using Hmz.Core;
using Hmz.Core.Input;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Scenes;
using Hmz.Core.Tilemap;

namespace Hmz.Editor;

public sealed class TilemapEditorScene : Scene
{
  const string MapPath = "maps/level1.json";

  readonly EditableTilemap tilemap = new();
  readonly EditorGhost ghost = new();
  readonly EditorCamera camera = new();

  public override void Initialize()
  {
    base.Initialize();

    ghost.Initialize();
    tilemap.Load(Engine.Content.Resolve(MapPath));
    ghost.CurrentLayer = Math.Clamp(ghost.CurrentLayer, 0, tilemap.Grid.LayerCount - 1);

    camera.Initialize(Engine.MainCamera);

    if (tilemap.Map != null) Instances.Add(tilemap.Map);
  }

  public override void Update(float dt)
  {
    base.Update(dt);

    ghost.HandleModeAndTypeSelection();
    ghost.HandleRotation();
    HandleLayer();
    HandleSave();
    if (Engine.Input.IsKeyJustPressed(Key.V)) camera.Cycle(Engine.MainCamera);
    ghost.HandleMovement(tilemap.Grid, dt);
    HandlePlacement();
    camera.Follow(Engine.MainCamera, ghost.WorldPosition(tilemap.Grid), dt);
  }

  // E past the top layer grows document.Grid.LayerCount rather than clamping - a map with
  // layerCount 1 has no higher layer to clamp to, so growing is what makes this key do
  // anything at all.
  void HandleLayer()
  {
    if (Engine.Input.IsKeyJustPressed(Key.E))
    {
      ghost.CurrentLayer++;
      if (ghost.CurrentLayer >= tilemap.Grid.LayerCount)
      {
        tilemap.Grid.LayerCount = ghost.CurrentLayer + 1;
      }
    }
    if (Engine.Input.IsKeyJustPressed(Key.Q))
    {
      ghost.CurrentLayer = Math.Max(ghost.CurrentLayer - 1, 0);
    }
  }

  // Saves to the tracked source file, then copies it over the bin output copy too - so a
  // save is visible immediately to anything in the current run that loads from bin
  // (e.g. switching to GameplayScene), without waiting for the next build to re-copy it.
  void HandleSave()
  {
    if (!Engine.Input.IsKeyJustPressed(Key.S))
    {
      return;
    }

    string sourcePath = Engine.Content.ResolveSource(MapPath);
    tilemap.Save(sourcePath);
    File.Copy(sourcePath, Engine.Content.Resolve(MapPath), overwrite: true);
  }

  void HandlePlacement()
  {
    if (Engine.Input.IsKeyJustPressed(Key.Space))
    {
      if (ghost.Mode == EditorMode.Tile && ghost.SelectedTileKey is { } tileKey)
      {
        tilemap.PlaceTile(ghost.CurrentLayer, ghost.CellX(tilemap.Grid), ghost.CellZ(tilemap.Grid), tileKey, ghost.RotationStep);
      }
      else if (ghost.Mode == EditorMode.Object && ghost.SelectedObjectKey is { } objectKey)
      {
        tilemap.PlaceObject(objectKey, ghost.WorldPosition(tilemap.Grid), new Vector3(0f, ghost.RotationStep * 90f, 0f));
      }
    }
    else if (Engine.Input.IsKeyJustPressed(Key.Backspace))
    {
      if (ghost.Mode == EditorMode.Tile)
      {
        tilemap.EraseTile(ghost.CurrentLayer, ghost.CellX(tilemap.Grid), ghost.CellZ(tilemap.Grid));
      }
      else
      {
        tilemap.EraseObject(ghost.WorldPosition(tilemap.Grid));
      }
    }
  }

  public override void Draw()
  {
    Engine.Graphics.StartMode3D(Engine.MainCamera);
    base.Draw();

    TilemapGrid grid = tilemap.Grid;
    float offsetX = grid.Width * grid.CellSize / 2f;
    float offsetZ = grid.Depth * grid.CellSize / 2f;
    Engine.Graphics.DrawDebugGrid(grid.Width, grid.Depth, grid.CellSize, ghost.CurrentLayer * grid.LayerHeight, offsetX, offsetZ);

    ghost.DrawPreview(grid);
    Engine.Graphics.EndMode3D();

    DrawHud();
  }

  void DrawHud()
  {
    TilemapGrid grid = tilemap.Grid;
    string modeLabel = ghost.Mode == EditorMode.Tile ? "Tile" : "Object";
    string selected = (ghost.Mode == EditorMode.Tile ? ghost.SelectedTileKey : ghost.SelectedObjectKey) ?? "(none registered)";
    string position = ghost.Mode == EditorMode.Tile
      ? $"Cell: ({ghost.CellX(grid)}, {ghost.CellZ(grid)})"
      : $"Pos: ({ghost.WorldPosition(grid).X:0.0}, {ghost.WorldPosition(grid).Z:0.0})";

    TextStyle style = new()
    {
      Color = Color.White,
      FontSize = 18f,
      Outline = new Stroke { Color = Color.Black, Width = 2f },
    };

    Engine.Graphics.DrawText($"Tilemap Editor - [{modeLabel}] mode - Camera: {camera.Preset}", 10f, 10f, style);
    Engine.Graphics.DrawText(
      $"Layer {ghost.CurrentLayer} (Y={ghost.CurrentLayer * grid.LayerHeight:0.0})   Selected: {selected}   Rot: {ghost.RotationStep * 90}deg   {position}",
      10f, 34f, style);
    Engine.Graphics.DrawText(
      "[Tab] type  [1/2] mode  [Q/E] layer  [R] rotate  [S] save  [Arrows] move (+Shift: 0.1 unit)  [Space] place  [Backspace] erase  [V] camera  [F3] exit",
      10f, 58f, style);
  }
}
