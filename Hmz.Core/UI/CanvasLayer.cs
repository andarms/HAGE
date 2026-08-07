using Hmz.Core.GOM;

namespace Hmz.Core.UI;

public sealed class CanvasLayer : GameObjectCollection
{
  public override void Add(GameObject item)
  {
    if (item is not UIElement)
    {
      throw new ArgumentException($"Only UIElement instances can be added to a CanvasLayer. Got {item.GetType()}.");
    }
    base.Add(item);
  }
}
