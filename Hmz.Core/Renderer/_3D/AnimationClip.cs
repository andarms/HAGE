namespace Hmz.Core.Renderer._3D;


public class AnimationClip
{
  public string Name { get; init; } = "";
  public float DurationTicks { get; init; }
  public float TicksPerSecond { get; init; } = 25f;
  public Dictionary<string, AnimationChannel> Channels { get; init; } = [];
}
