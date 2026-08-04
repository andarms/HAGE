namespace Hmz.Core.States;

public abstract class State<TContext>
{
    public virtual IEnumerable<Transition<TContext>> Transitions => [];

    public virtual void Enter(TContext context) { }

    public virtual void Update(TContext context, float dt) { }

    public virtual void Exit(TContext context) { }

    public virtual bool IsBusy() => false;
}
