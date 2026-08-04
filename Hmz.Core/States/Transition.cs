namespace Hmz.Core.States;

public abstract class Transition<TContext>
{
  public abstract Type Target { get; }

  public abstract bool ShouldTransition(TContext context);
}

public abstract class Transition<TContext, TTarget> : Transition<TContext> where TTarget : State<TContext>
{
  public sealed override Type Target => typeof(TTarget);
}