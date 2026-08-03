namespace Hmz.Core.Hosting;

public static class Performance
{
  const int SampleCount = 30;

  static readonly float[] samples = new float[SampleCount];
  static int sampleIndex;
  static int sampleFill;
  static float sampleSum;

  public static int FPS { get; private set; }
  public static float FrameTimeMs { get; private set; }

  internal static void Sample(float deltaSeconds)
  {
    FrameTimeMs = deltaSeconds * 1000f;

    sampleSum -= samples[sampleIndex];
    samples[sampleIndex] = deltaSeconds;
    sampleSum += deltaSeconds;
    sampleIndex = (sampleIndex + 1) % SampleCount;
    sampleFill = Math.Min(sampleFill + 1, SampleCount);

    float averageDelta = sampleSum / sampleFill;
    FPS = averageDelta > 0f ? (int)MathF.Round(1f / averageDelta) : 0;
  }

  public static void Reset()
  {
    Array.Clear(samples);
    sampleIndex = 0;
    sampleFill = 0;
    sampleSum = 0f;
    FPS = 0;
    FrameTimeMs = 0f;
  }
}
