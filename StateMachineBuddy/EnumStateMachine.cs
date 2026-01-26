namespace StateMachineBuddy
{
    /// <summary>
    /// A type-safe state machine implementation using enum types for states and messages.
    /// </summary>
    /// <typeparam name="TState">The enum type representing states.</typeparam>
    /// <typeparam name="TMessage">The enum type representing messages.</typeparam>
    public class EnumStateMachine<TState, TMessage> : StringStateMachine
    {
        /// <summary>
        /// Initializes a new instance of the EnumStateMachine class with the specified initial state.
        /// </summary>
        /// <param name="initialState">The initial state of the state machine.</param>
        public EnumStateMachine(TState initialState)
        {
            AddStateMachine(typeof(TState), typeof(TMessage), initialState.ToString());
        }

        /// <summary>
        /// Defines a state transition.
        /// </summary>
        /// <param name="state">The source state.</param>
        /// <param name="message">The message that triggers the transition.</param>
        /// <param name="nextState">The target state after the transition.</param>
        public void Set(TState state, TMessage message, TState nextState)
        {
            Set(state.ToString(), message.ToString(), nextState.ToString());
        }
    }
}
