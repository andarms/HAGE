namespace Hmz.Core.GOM;

public sealed class ChildCollection(GameObject owner) : GameObjectCollection
{
  public override void Add(GameObject item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (ReferenceEquals(item, owner))
    {
      throw new InvalidOperationException("A GameObject cannot be its own child.");
    }

    for (GameObject? ancestor = owner; ancestor != null; ancestor = ancestor.Parent)
    {
      if (ReferenceEquals(ancestor, item))
      {
        throw new InvalidOperationException("A GameObject cannot be added below one of its descendants.");
      }
    }

    if (ReferenceEquals(item.Parent, owner))
    {
      return;
    }

    item.Parent?.Children.Remove(item);
    base.Add(item);
    item.Parent = owner;
  }

  public override void Remove(GameObject item)
  {
    base.Remove(item);
    if (ReferenceEquals(item.Parent, owner))
    {
      item.Parent = null;
    }
  }
}
