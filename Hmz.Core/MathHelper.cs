namespace Hmz.Core;

public static class MathHelper
{
  // Lerps by the shortest angular path, so crossing the -pi/pi wraparound doesn't spin the long way.
  public static float LerpAngle(float from, float to, float t)
  {
    float delta = ((to - from + MathF.PI) % MathF.Tau + MathF.Tau) % MathF.Tau - MathF.PI;
    return from + delta * t;
  }

  public static T SampleKeyframes<T>(IReadOnlyList<(float Time, T Value)> keys, float time, T fallback, Func<T, T, float, T> interpolate)
  {
    if (keys.Count == 0) return fallback;
    if (keys.Count == 1 || time <= keys[0].Time) return keys[0].Value;
    if (time >= keys[^1].Time) return keys[^1].Value;

    for (int i = 0; i < keys.Count - 1; i++)
    {
      if (time <= keys[i + 1].Time)
      {
        float span = keys[i + 1].Time - keys[i].Time;
        float t = span > 0f ? (time - keys[i].Time) / span : 0f;
        return interpolate(keys[i].Value, keys[i + 1].Value, t);
      }
    }
    return keys[^1].Value;
  }
}
