
namespace StateMachineBuddy
{
    public class EnumStateMachine<TState, TMessage> : StringStateMachine
    {
        public EnumStateMachine(TState initialState)
        {
            AddStateMachine(typeof(TState), typeof(TMessage), initialState.ToString());
        }

        public void Set(TState state, TMessage message, TState nextState)
        {
            Set(state.ToString(), message.ToString(), nextState.ToString());
        }
    }
}
