namespace Hmz.Core.Hosting;

public record GameOptions
{
  public int Width { get; init; } = 800;
  public int Height { get; init; } = 600;
  public string Title { get; init; } = "Hamaze";
  public int TargetFps { get; init; } = 60;
}
