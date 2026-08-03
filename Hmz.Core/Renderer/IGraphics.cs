using System.Numerics;
using Hmz.Core._2D;
using Hmz.Core._3D;
using Hmz.Core._3D.Geometry;
using Hmz.Core.Renderer.Styles;

namespace Hmz.Core.Renderer;

public interface IGraphics : IDisposable
{
  void Clear(Color color);
  void StartFrame();
  void EndFrame();
  void Resize(int width, int height);

  // GL draw calls in the last completed frame; resets on StartFrame.
  int DrawCallCount { get; }

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
  void DrawModel(Model model, Matrix4x4 worldMatrix, Matrix4x4[] boneMatrices);
  void DrawDebugGrid(int xRows, int zColumns, float cellSize);
  void DrawSphere(Sphere sphere, SphereStyle style);
  #endregion

  #region Text Drawing
  void DrawText(string text, float x, float y, TextStyle style);
  #endregion
}