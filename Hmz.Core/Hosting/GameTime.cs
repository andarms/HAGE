namespace Hmz.Core.Hosting;

public static class GameTime
{
  // Keeps TotalTime bounded so accumulated float error doesn't start dropping sub-frame
  // increments during long sessions (float mantissa gives ~7 significant digits).
  const float WrapAfterSeconds = 86400f; // 24 hours

  public static float DeltaTime { get; private set; }
  public static float TotalTime { get; private set; }

  internal static void Advance(float deltaTime)
  {
    DeltaTime = deltaTime;
    TotalTime += deltaTime;
    if (TotalTime >= WrapAfterSeconds)
    {
      TotalTime -= WrapAfterSeconds;
    }
  }

  public static void Reset()
  {
    DeltaTime = 0f;
    TotalTime = 0f;
  }
}
