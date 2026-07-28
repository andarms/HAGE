using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Core;
using Hmz.Core.Graphics;
using Hmz.Core.Graphics._3D;
using System.Numerics;

namespace Hmz.Core;

public record GameOptions
{
  public int Width { get; init; } = 800;
  public int Height { get; init; } = 600;
  public string Title { get; init; } = "Hamaze";
}

public class Game
{
  readonly IWindow window;
  IInputContext input;
  private GraphicsRenderer renderer;
  private int width;
  private int height;

  readonly Camera3D camera;
  readonly Cube cube;


  public Game(GameOptions? options = null)
  {
    options ??= new GameOptions();
    width = options.Width;
    height = options.Height;

    WindowOptions windowOptions = WindowOptions.Default;
    windowOptions.Size = new Vector2D<int>(options.Width, options.Height);
    windowOptions.Title = options.Title;
    windowOptions.API = new GraphicsAPI(
      ContextAPI.OpenGL,
      ContextProfile.Core,
      ContextFlags.ForwardCompatible,
      new APIVersion(3, 3)
    );

    camera = new Camera3D
    {
      Position = new(0, 8.0f, 9.0f),
      Target = new Vector3(0f, 0f, 0f),
      Up = Vector3.UnitY,
      FieldOfView = MathF.PI / 4f,
    };
    camera.Rotate(0f, 0f);

    cube = new Cube() { Size = 2f };

    window = Window.Create(windowOptions);

    window.Load += Initialize;
    window.Update += Update;
    window.Render += Render;
    window.FramebufferResize += OnFramebufferResize;
    window.Closing += OnClose;
  }

  public void Run()
  {
    Engine.Current = this;
    window.Run();
    window.Dispose();
  }

  private void Initialize()
  {
    input = window.CreateInput();
    for (int i = 0; i < input.Keyboards.Count; i++)
    {
      input.Keyboards[i].KeyDown += KeyDown;
    }

    Engine.GL = GL.GetApi(window);
    renderer = new GraphicsRenderer(width, height);

    window.FramebufferResize += ResizeViewport;
  }

  void ResizeViewport(Vector2D<int> size)
  {
    width = size.X;
    height = size.Y;
    Engine.GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    renderer.Resize(size.X, size.Y);
  }

  private void Update(double delta)
  {
    camera.Orbit(Vector3.Zero, 0.5f * (float)delta, 0);
  }

  private void Render(double delta)
  {
    renderer.BeginFrame(Color.CornflowerBlue);
    renderer.DrawRectangle(100, 100, 200, 150, Color.Red);
    renderer.DrawRectangle(350, 250, 120, 300, Color.Blue);
    renderer.End();

    renderer.BeginFrame(camera);
    renderer.DrawCube(cube, Color.Red);
    renderer.DrawCubeWires(cube, Color.Green);
    renderer.EndFrame();


    var err = Engine.GL.GetError();
    if (err != GLEnum.NoError)
    {
      Console.WriteLine($"GL Error: {err}");
    }
  }

  private void OnFramebufferResize(Vector2D<int> newSize)
  {
    Engine.GL.Viewport(newSize);
    renderer.Resize(newSize.X, newSize.Y);
  }

  private void OnClose()
  {
    renderer.Dispose();
  }

  private void KeyDown(IKeyboard keyboard, Key key, int scancode)
  {
    if (key == Key.Escape)
    {
      window.Close();
    }
  }
}

