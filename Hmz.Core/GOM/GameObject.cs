using Hmz.Core._3D.Geometry;
using Hmz.Core.Collisions;
using Hmz.Core.Renderer;
using Hmz.Core.Renderer.Styles;
using Hmz.Core.Spatial;
using System.Numerics;

namespace Hmz.Core.GOM;

public class GameObject
{

  public ChildCollection Children { get; }

  public GameObject? Parent { get; internal set; }

  public Transform Transform { get; } = new();

  Matrix4x4 cachedWorldMatrix = Matrix4x4.Identity;
  int cachedLocalVersion = -1;
  int cachedParentVersion = -1;
  GameObject? cachedParent;
  int worldVersion;

  Transform? cachedWorldTransform;
  int cachedWorldTransformVersion = -1;

  public Matrix4x4 WorldMatrix
  {
    get
    {
      RefreshWorldMatrix();
      return cachedWorldMatrix;
    }
  }

  internal int WorldVersion
  {
    get
    {
      RefreshWorldMatrix();
      return worldVersion;
    }
  }

  void RefreshWorldMatrix()
  {
    int parentVersion = Parent?.WorldVersion ?? 0;
    if (Transform.Version == cachedLocalVersion && parentVersion == cachedParentVersion && ReferenceEquals(Parent, cachedParent))
    {
      return;
    }

    cachedWorldMatrix = Transform.GetLocalMatrix() * (Parent?.WorldMatrix ?? Matrix4x4.Identity);
    cachedLocalVersion = Transform.Version;
    cachedParentVersion = parentVersion;
    cachedParent = Parent;
    worldVersion++;
  }

  public Transform WorldTransform
  {
    get
    {
      RefreshWorldMatrix();
      if (cachedWorldTransform == null || cachedWorldTransformVersion != worldVersion)
      {
        cachedWorldTransform = Transform.FromMatrix(cachedWorldMatrix);
        cachedWorldTransformVersion = worldVersion;
      }
      return cachedWorldTransform;
    }
  }

  public Vector3 GlobalPosition => Vector3.Transform(Vector3.Zero, WorldMatrix);

  public Vector3 Forward
  {
    get
    {
      Vector3 direction = Vector3.TransformNormal(Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(Transform.Rotation));
      direction.Y = 0f;
      return Vector3.Normalize(direction);
    }
  }

  public bool IsActive { get; protected set; } = true;

  public int DrawOrder { get; set; }

  public ComponentCollection Components { get; }

  public Collider? Collider { get; protected set; }

  public GameObject()
  {
    Children = new ChildCollection(this);
    Components = new ComponentCollection(this);
  }

  public void RemoveFromParent() => Parent?.Children.Remove(this);

  public virtual void Initialize()
  {
    if (Collider != null) Engine.Collisions.Register(this);

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
    foreach (GameObject child in Children.OrderBy(i => i.DrawOrder))
    {
      child.Draw();
      if (Engine.DebugMode) child.Debug();
    }
    Components.Draw();
  }

  public virtual void Debug()
  {
    Engine.Graphics.DrawSphere(new Sphere(GlobalPosition, 0.1f), new SphereStyle { Wireframe = true, Color = Color.Red });

    if (Collider != null)
    {
      Color color = Collider.Type == CollisionType.Solid ? Color.Red : Color.Yellow;
      var cube = Collider.Bounds(GlobalPosition).ToCube();
      Engine.Graphics.DrawCube(cube, new CubeStyle { Wireframe = true, Color = color });
    }
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
    if (Collider != null) Engine.Collisions.Unregister(this);
    IsActive = false;
    Components.Terminate();
    Children.Clear();
    Parent = null;
  }
}
