using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Adds transitions from a state model to this state.
        /// </summary>
        /// <param name="stateTable">The state model containing transitions to add.</param>
        /// <param name="stateMachine">The parent state machine for validation.</param>
        /// <param name="stateName">The name of this state (used to filter self-transitions).</param>
        public void AddStateMachine(StateModel stateTable, StringStateMachine stateMachine, string stateName)
        {
            foreach (var change in stateTable.Transitions)
            {
                if (!stateMachine.Messages.Contains(change.Message))
                {
                    throw new Exception($"State machine doesn't have message {change.Message}");
                }
                if (!stateMachine.States.Contains(change.TargetState))
                {
                    throw new Exception($"State machine doesn't have state {change.TargetState}");
                }

                if (stateName != change.TargetState)
                {
                    StateChanges[change.Message] = change.TargetState;
                }
            }
        }

        /// <summary>
        /// Removes transitions that reference messages or states from the specified model.
        /// </summary>
        /// <param name="stateModel">The state machine model whose messages and states should be removed.</param>
        public void RemoveStateMachine(StateMachineModel stateModel)
        {
            //remove the messages
            foreach (var message in stateModel.MessageNames)
            {
                StateChanges.Remove(message);
            }

            //remove the targetstates
            foreach (var state in stateModel.StateNames)
            {
                foreach (var targetState in StateChanges.Where(x => x.Value == state).ToList())
                {
                    StateChanges.Remove(targetState.Key);
                }
            }
        }

        #endregion //Methods
    }
}
