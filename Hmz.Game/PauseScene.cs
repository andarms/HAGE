using Hmz.Core;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Scenes;
using Hmz.Core.UI;

namespace Hmz.Game;

public sealed class PauseScene : Scene
{
  public override void Initialize()
  {
    base.Initialize();

    ColorRect backdrop = new() { Style = new RectangleStyle { Fill = new Color(0, 0, 0, 160) } };

    MarginContainer root = new();
    root.FillParent();
    root.SetMargin(40f);

    CenterContainer center = new();
    center.FillParent();

    VBoxContainer menu = new() { Spacing = 12f };
    menu.Children.Add(new Label { Text = "Paused", Style = new TextStyle { FontSize = 32f } });

    Button resume = new() { Text = "Resume", HorizontalAlignment = UIAlignment.Stretch, TextStyle = new TextStyle { FontSize = 32f } };
    resume.Clicked += () => Engine.Scenes.Pop();
    menu.Children.Add(resume);

    center.Children.Add(menu);
    root.Children.Add(center);
    backdrop.Children.Add(root);
    Canvas.Add(backdrop);
  }
}
