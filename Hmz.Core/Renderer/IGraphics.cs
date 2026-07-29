using System.Numerics;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer._2D;
using Hmz.Core.Renderer._3D;

namespace Hmz.Core;

public record Stroke { public Color Color { get; init; } = Color.Black; public float Width { get; init; } = 1f; }
public record RectangleStyle { public Color? Fill { get; init; } = Color.White; public Stroke? Border { get; init; } public float CornerRadius { get; init; } }
public record CircleStyle { public Color? Fill { get; init; } = Color.White; public Stroke? Border { get; init; } }
public record CubeStyle
{
  public Color Color { get; init; } = Color.White;
  public float Width { get; init; } = 1f;
  public bool Wireframe { get; init; } = false;
  public Stroke? Border { get; init; } = new Stroke { Color = Color.Black, Width = 1f };
}


public record TextStyle
{
  public Color Color { get; init; } = Color.White;
  public float FontSize { get; init; } = 12f;
  public Font? Font { get; init; }
  public Stroke? Outline { get; init; }
}

public interface IGraphics
{
  void Clear(Color color);
  void StartFrame();
  void EndFrame();

  #region 2D Drawing

  void StartMode2D(Camera2D camera);
  void EndMode2D();


  void DrawRectangle(float x, float y, float width, float height, RectangleStyle style);
  void DrawRectangle(Rectangle bounds, RectangleStyle style);
  void DrawLine(float x1, float y1, float x2, float y2, Stroke style);
  void DrawLine(Vector2 start, Vector2 end, Stroke style);
  void DrawLines(Vector2[] points, Stroke style);
  void DrawCircle(float centerX, float centerY, float radius, CircleStyle style);
  void DrawCircle(Vector2 center, float radius, CircleStyle style);
  void DrawTexture(Texture2D texture, float x, float y, float width, float height);
  #endregion

  #region 3D Drawing
  void StartMode3D(Camera3D camera);
  void EndMode3D();


  void DrawCube(Cube cube, CubeStyle style);
  void DrawModel(Model model);
  #endregion

  #region Text Drawing
  void DrawText(string text, float x, float y, TextStyle style);
  #endregion
}