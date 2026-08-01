using Hmz.Core.Collisions;
using Hmz.Core.Spatial;
using System.Numerics;

namespace Hmz.Core.GOM;

public class GameObject
{

  public ChildCollection Children { get; }

  public GameObject? Parent { get; internal set; }

  public Transform Transform { get; } = new();

  public Matrix4x4 WorldMatrix => Transform.GetLocalMatrix() * (Parent?.WorldMatrix ?? Matrix4x4.Identity);

  public Transform WorldTransform => Transform.FromMatrix(WorldMatrix);

  public Vector3 GlobalPosition => Vector3.Transform(Vector3.Zero, WorldMatrix);

  public bool IsActive { get; protected set; } = true;

  public int DrawOrder { get; set; }

  public ComponentCollection Components { get; }

  public Collider? Collider { get; }

  public GameObject()
  {
    Children = new ChildCollection(this);
    Components = new ComponentCollection(this);
  }

  public void RemoveFromParent() => Parent?.Children.Remove(this);

  public virtual void Initialize()
  {
    foreach (GameObject child in Children)
    {
      child.Initialize();
    }
  }

  public virtual void HandleInput()
  {
    Components.HandleInput();
    foreach (GameObject child in Children)
    {
      child.HandleInput();
    }
  }

  public virtual void Update(float dt)
  {
    Components.Update(dt);
    Children.Update(dt, onAdded: child => child.Initialize(), onRemoved: child => child.Terminate());
  }

  public virtual void Draw()
  {
    foreach (GameObject child in Children.OrderBy(i => i.DrawOrder).ThenBy(i => i.WorldTransform.Position.Y))
    {
      child.Draw();
      if (Engine.DebugMode) child.Debug();
    }
    Components.Draw();
  }

  public virtual void Debug()
  {
    foreach (GameObject child in Children)
    {
      child.Debug();
    }
    Components.Debug();
  }

  public virtual void Terminate()
  {
    foreach (GameObject child in Children)
    {
      child.Terminate();
      child.Parent = null;
    }
    IsActive = false;
    Components.Terminate();
    Children.Clear();
    Parent = null;
  }
}
