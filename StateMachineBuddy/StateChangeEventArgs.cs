using System;

namespace StateMachineBuddy
{
    /// <summary>
    /// custom event argument that includes the previous and new state
    /// </summary>
    public class StateChangeEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The previous state
        /// </summary>
        public T OldState { get; set; }

        /// <summary>
        /// the new state we just changed to
        /// </summary>
        public T NewState { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateChangeEventArgs(T oldState, T newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
