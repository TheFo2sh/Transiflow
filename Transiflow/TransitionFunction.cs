using Transiflow;

namespace Transiflow;

public delegate Task<IState<TStateTag>> TransitionFunction<TStateTag, TEventTag, TContext>(
    IState<TStateTag> currentState, IEvent<TEventTag> ev, TContext context);