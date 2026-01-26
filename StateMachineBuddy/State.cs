using System.Collections.Generic;

namespace StateMachineBuddy
{
    /// <summary>
    /// Represents a state in the state machine with its associated transitions.
    /// </summary>
    public class State
    {
        #region Properties

        /// <summary>
        /// Gets or sets the dictionary of state transitions, where the key is the message
        /// and the value is the target state name.
        /// </summary>
        public Dictionary<string, string> StateChanges { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the State class.
        /// </summary>
        public State()
        {
            StateChanges = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the State class by copying another state.
        /// </summary>
        /// <param name="state">The state to copy.</param>
        public State(State state) : this()
        {
            foreach (var stateChange in state.StateChanges)
            {
                StateChanges.Add(stateChange.Key, stateChange.Value);
            }
        }

        #endregion //Methods
    }
}
