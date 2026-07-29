using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

using System.Numerics;
using Hmz.Core.Renderer._3D;
using Hmz.Core.Renderer;
using Hmz.Core.Content;
using Hmz.Core.Renderer._2D;

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
  private Renderer.Graphics renderer;
  private int width;
  private int height;

  readonly Camera3D camera;
  readonly Cube cube;

  readonly ContentManager content = new ContentManager();
  Texture2D texture;
  Model treeModel;

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
    renderer = new Renderer.Graphics(width, height);

    window.FramebufferResize += ResizeViewport;

    texture = content.LoadTexture("textures/tiny_dungeon.png");
    treeModel = content.LoadModel("models/tree_1.gltf");
    treeModel.Transform.Position = new Vector3(2f, 0f, 2f);
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
    renderer.Clear(Color.CornflowerBlue);
    renderer.StartFrame();

    renderer.StartMode3D(camera);
    renderer.DrawModel(treeModel);
    renderer.DrawCube(cube, new CubeStyle { Color = Color.Red, Wireframe = true });
    renderer.EndMode3D();

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
    treeModel.Dispose();
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

