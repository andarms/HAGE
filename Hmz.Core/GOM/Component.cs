namespace Hmz.Core.GOM;

public class Component
{
  GameObject? owner;
  protected GameObject Owner => owner ?? throw new InvalidOperationException("Component is not attached to a GameObject.");

  public bool Enabled { get; set; } = true;

  public void Attach(GameObject owner)
  {
    ArgumentNullException.ThrowIfNull(owner);

    if (ReferenceEquals(this.owner, owner))
    {
      return;
    }
    this.owner = owner;
  }

  public void Detach()
  {
    owner = null;
  }

  public virtual void Initialize() { }

  public virtual void HandleInput() { }

  public virtual void Update(float dt) { }

  public virtual void Draw() { }

  public virtual void Debug() { }

  public virtual void Terminate() { }
}