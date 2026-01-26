using System.Collections.Generic;

namespace StateMachineBuddy
{
    public class State
    {
        #region Properties

        public Dictionary<string, string> StateChanges { get; set; }

        #endregion //Properties

        #region Methods

        public State()
        {
            StateChanges = new Dictionary<string, string>();
        }

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
