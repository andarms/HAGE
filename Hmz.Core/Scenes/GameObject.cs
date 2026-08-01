using Hmz.Core;
using Hmz.Core.Renderer._3D;
using System.Numerics;

namespace Hmz.Core.Scenes;

public class GameObject
{
  readonly List<GameObject> children = [];

  public GameObject? Parent { get; private set; }

  public IReadOnlyList<GameObject> Children => children.AsReadOnly();

  public Transform Transform { get; } = new();

  public Matrix4x4 WorldMatrix => Transform.GetLocalMatrix() * (Parent?.WorldMatrix ?? Matrix4x4.Identity);

  public Transform WorldTransform => Transform.FromMatrix(WorldMatrix);

  public Vector3 GlobalPosition => Vector3.Transform(Vector3.Zero, WorldMatrix);

  public bool IsActive { get; set; } = true;

  public int DrawOrder { get; set; }

  public void Add(GameObject child)
  {
    ArgumentNullException.ThrowIfNull(child);

    if (ReferenceEquals(child, this))
    {
      throw new InvalidOperationException("A GameObject cannot be its own child.");
    }

    for (GameObject? ancestor = this; ancestor != null; ancestor = ancestor.Parent)
    {
      if (ReferenceEquals(ancestor, child))
      {
        throw new InvalidOperationException("A GameObject cannot be added below one of its descendants.");
      }
    }

    if (ReferenceEquals(child.Parent, this))
    {
      return;
    }

    child.Parent?.Remove(child);
    children.Add(child);
    child.Parent = this;
  }

  public bool Remove(GameObject child)
  {
    ArgumentNullException.ThrowIfNull(child);

    if (!children.Remove(child))
    {
      return false;
    }

    child.Parent = null;
    return true;
  }

  public void RemoveFromParent() => Parent?.Remove(this);

  public virtual void Initialize()
  {
    foreach (GameObject child in children)
    {
      child.Initialize();
    }
  }

  public virtual void HandleInput()
  {
    foreach (GameObject child in children)
    {
      child.HandleInput();
    }
  }

  public virtual void Update(float dt)
  {
    foreach (GameObject child in children)
    {
      if (child.IsActive)
      {
        child.Update(dt);
      }
    }
  }

  public virtual void Draw()
  {
    foreach (GameObject child in children.OrderBy(i => i.DrawOrder).ThenBy(i => i.WorldTransform.Position.Y))
    {
      child.Draw();
      if (Engine.DebugMode) child.Debug();
    }
  }

  public virtual void Debug() { }

  public virtual void Terminate()
  {
    foreach (GameObject child in children)
    {
      child.Terminate();
      child.Parent = null;
    }

    children.Clear();
    Parent = null;
  }
}