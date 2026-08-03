using Hmz.Core.Hosting;
using Hmz.Game;
using Hmz.Windows;

GameHost host = new(
  new UntitledGame(),
  new GameOptions
  {
    Title = "Hamaze is fun",
    Width = 640,
    Height = 480,
    TargetFps = 120,
  },
  new WindowsPlatformWindowService()
);
host.Run();
