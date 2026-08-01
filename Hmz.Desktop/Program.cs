using Hmz.Core;
using Hmz.Game;

Game game = new(new GameplayScene(), new GameOptions { Title = "Hamaze is fun", Width = 1280, Height = 720 });
game.Run();
