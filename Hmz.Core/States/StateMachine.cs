using Hmz.Core.GOM;

namespace Hmz.Core.States;

public sealed class StateMachine<TContext>(TContext context) : Component
{
  private readonly Dictionary<Type, State<TContext>> states = [];

  private readonly TContext context = context;

  private State<TContext> current = null!;

  private Type? pending;

  public State<TContext> Current => current;

  public void Register(State<TContext> state)
  {
    states[state.GetType()] = state;
  }

  public void Start<TState>() where TState : State<TContext>
  {
    current = states[typeof(TState)];
    current.Enter(context);
  }

  public override void Update(float dt)
  {
    current.Update(context, dt);
    if (!current.IsBusy()) EvaluateTransitions();
    ApplyPendingTransition();
  }

  private void EvaluateTransitions()
  {
    if (pending != null) return;

    foreach (var transition in current.Transitions)
    {
      if (transition.ShouldTransition(context))
      {
        pending = transition.Target;
        break;
      }
    }
  }

  private void ApplyPendingTransition()
  {
    if (pending == null) return;

    current.Exit(context);

    current = states[pending];
    pending = null;
    current.Enter(context);
  }
}