using System;
using System.Collections.Generic;
using System.Linq;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace StateMachineBuddy
{
    /// <summary>
    /// A flexible state machine implementation using string-based states and messages.
    /// </summary>
    public class StringStateMachine
    {
        #region Properties

        /// <summary>
        /// The default state of this state machine.
        /// </summary>
        public string InitialState { get; set; }

        /// <summary>
        /// The previous state of the state machine.
        /// </summary>
        public string PrevState { get; private set; }

        /// <summary>
        /// The current state of the state machine
        /// </summary>
        public string CurrentState { get; private set; }

        /// <summary>
        /// All the states and state changes of this state machine.
        /// </summary>
        public Dictionary<string, State> StateTable { get; private set; } = new Dictionary<string, State>();

        /// <summary>
        /// A list of all the possible messages in this state machine
        /// Only used for validation purposes!
        /// </summary>
        public HashSet<string> Messages { get; private set; } = new HashSet<string>();

        /// <summary>
        /// A list of all the possible states in this state machine
        /// Only used for validation purposes!
        /// </summary>
        public HashSet<string> States { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Event that gets fired when this state machine is reset
        /// </summary>
        public event EventHandler<StateChangeEventArgs<string>> ResetEvent;

        /// <summary>
        /// Event that gets fired every time the state changes
        /// </summary>
        public event EventHandler<StateChangeEventArgs<string>> StateChangedEvent;

        #endregion //Properties

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the StringStateMachine class.
        /// </summary>
        public StringStateMachine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the StringStateMachine class by copying another state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine to copy.</param>
        public StringStateMachine(StringStateMachine stateMachine)
        {
            //add all the state changes
            foreach (var state in stateMachine.StateTable)
            {
                StateTable.Add(state.Key, new State(state.Value));
            }

            //add all the state names
            AddStates(stateMachine.States);

            //add all the message names
            AddMessages(stateMachine.Messages);

            //set the initial state
            SetInitialState(stateMachine.InitialState);
        }

        /// <summary>
        /// Sets the initial state of the state machine.
        /// </summary>
        /// <param name="initialState">The name of the initial state.</param>
        public void SetInitialState(string initialState)
        {
            if (States.Contains(initialState))
            {
                InitialState = initialState;
                PrevState = initialState;
                CurrentState = initialState;
            }
        }

        /// <summary>
        /// Adds multiple states to the state machine.
        /// </summary>
        /// <param name="states">The collection of state names to add.</param>
        public void AddStates(IEnumerable<string> states)
        {
            //add all the states
            foreach (var stateName in states)
            {
                //double check the state name
                if (!States.Contains(stateName))
                {
                    States.Add(stateName);
                }

                if (!StateTable.ContainsKey(stateName))
                {
                    StateTable[stateName] = new State();
                }
            }
        }

        /// <summary>
        /// Adds multiple messages to the state machine.
        /// </summary>
        /// <param name="messages">The collection of message names to add.</param>
        public void AddMessages(IEnumerable<string> messages)
        {
            //double check all messages
            foreach (var messageName in messages)
            {
                if (!Messages.Contains(messageName))
                {
                    Messages.Add(messageName);
                }
            }
        }

        /// <summary>
        /// Initializes the state machine using enum types for states and messages.
        /// </summary>
        /// <param name="statesEnum">The enum type defining the states.</param>
        /// <param name="messagesEnum">The enum type defining the messages.</param>
        /// <param name="initialState">The name of the initial state.</param>
        public void AddStateMachine(Type statesEnum, Type messagesEnum, string initialState)
        {
            AddStates(statesEnum);
            AddMessages(messagesEnum);
            AddMessages(Enum.GetNames(messagesEnum).ToList());

            SetInitialState(initialState);
        }

        /// <summary>
        /// Adds states from an enum type.
        /// </summary>
        /// <param name="statesEnum">The enum type defining the states.</param>
        public void AddStates(Type statesEnum)
        {
            AddStates(Enum.GetNames(statesEnum).ToList());
        }

        /// <summary>
        /// Adds messages from an enum type.
        /// </summary>
        /// <param name="messagesEnum">The enum type defining the messages.</param>
        public void AddMessages(Type messagesEnum)
        {
            AddMessages(Enum.GetNames(messagesEnum).ToList());
        }

        /// <summary>
        /// Sets an state in the state table to respond to a particular message
        /// </summary>
        /// <param name="state">State to set up a message for</param>
        /// <param name="message">message to parse</param>
        /// <param name="nextState">state this state will change to after getting the message</param>
        public void Set(string state, string message, string nextState)
        {
            if (!States.Contains(state))
            {
                throw new Exception($"States does not contain the state {state}");
            }
            else if (!StateTable.ContainsKey(state))
            {
                throw new Exception($"StateTable does not contain the state {state}");
            }
            else if (!Messages.Contains(message))
            {
                throw new Exception($"Messages does not contain the state {message}");
            }
            else if (!States.Contains(nextState))
            {
                throw new Exception($"States does not contain the state {nextState}");
            }

            StateTable[state].StateChanges[message] = nextState;
        }

        /// <summary>
		/// Add all the states and state transitions from a StateMachine object
		/// </summary>
		/// <param name="stateModel"></param>
		public void AddStateMachine(StateMachineModel stateModel)
        {
            AddMessages(stateModel.MessageNames);
            AddStates(stateModel.StateNames);

            //add all the state transitions
            foreach (var stateTableModel in stateModel.States)
            {
                //find the matching state
                if (!StateTable.ContainsKey(stateTableModel.Name))
                {
                    throw new Exception($"State machine is missing state definitions for {stateTableModel.Name}");
                }

                var state = StateTable[stateTableModel.Name];
                state.AddStateMachine(stateTableModel, this, stateTableModel.Name);
            }
        }

        /// <summary>
		/// Remove all the states and state transitions from a StateMachine object
		/// </summary>
		/// <param name="stateMachine"></param>
		public void RemoveStateMachine(StateMachineModel stateModel)
        {
            //remove all the messages that have been added
            foreach (var messageName in stateModel.MessageNames)
            {
                Messages.Remove(messageName);
            }

            foreach (var stateName in stateModel.StateNames)
            {
                States.Remove(stateName);
            }

            //remove all the states in the state machine that is being removed
            foreach (var stateName in stateModel.StateNames)
            {
                StateTable.Remove(stateName);
            }

            //clean up all the state transitions that were changed
            foreach (var state in StateTable)
            {
                state.Value.RemoveStateMachine(stateModel);
            }

            //Reset the initial state if necessary
            if (stateModel.StateNames.Contains(CurrentState))
            {
                ForceState(InitialState);
            }
        }

        #endregion //Initialization

        #region Methods

        /// <summary>
        /// This function sets the thing to its initial state.
        /// </summary>
        public void ResetToInitialState()
        {
            PrevState = InitialState;
            CurrentState = InitialState;

            //Send the reset event instread of the state change event
            ResetEvent?.Invoke(this, new StateChangeEventArgs<string>(PrevState, CurrentState));
        }

        /// <summary>
        /// Sends a message to the state machine, potentially triggering a state transition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the state changed, false otherwise.</returns>
        public virtual bool SendStateMessage(string message)
        {
            //get the current state
            if (StateTable.ContainsKey(CurrentState))
            {
                var state = StateTable[CurrentState];

                //Does the current state listen for this message?
                if (state.StateChanges.ContainsKey(message))
                {
                    var nextState = state.StateChanges[message];
                    if (nextState != CurrentState)
                    {
                        //Set the previous state
                        PrevState = CurrentState;

                        //Set the current state
                        CurrentState = nextState;

                        //Fire off the state change event
                        OnStateChange(PrevState, CurrentState);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Method to force the state machine to a certain state
        /// </summary>
        /// <param name="state">state to set machine to</param>
        public void ForceState(string state)
        {
            //will the state change after forcing it?
            if ((state != CurrentState) && StateTable.ContainsKey(state))
            {
                //set the previous state
                PrevState = CurrentState;
                CurrentState = state;

                //fire off the event
                OnStateChange(PrevState, CurrentState);
            }
        }

        /// <summary>
        /// Method for raising the state change event.
        /// </summary>
        /// <param name="oldState">the state changing from</param>
        /// <param name="nextState">the next state</param>
        protected virtual void OnStateChange(string oldState, string nextState)
        {
            StateChangedEvent?.Invoke(this, new StateChangeEventArgs<string>(oldState, nextState));
        }

        #endregion //Methods

        #region File IO

        /// <summary>
        /// Loads the state machine configuration from an XML file.
        /// </summary>
        /// <param name="file">The path to the XML file.</param>
        /// <param name="xmlContent">The content manager for loading the XML.</param>
        public void LoadXml(Filename file, ContentManager xmlContent)
        {
            //Load the model
            using (var stateMachineModel = new StateMachineModel(file))
            {
                stateMachineModel.ReadXmlFile(xmlContent);

                LoadStateMachine(stateMachineModel);
            }
        }

        /// <summary>
        /// Loads the state machine from a model object.
        /// </summary>
        /// <param name="stateMachineModel">The model containing state machine data.</param>
        public void LoadStateMachine(StateMachineModel stateMachineModel)
        {
            AddStates(stateMachineModel.StateNames);
            AddMessages(stateMachineModel.MessageNames);
            SetInitialState(stateMachineModel.Initial);

            foreach (var state in stateMachineModel.States)
            {
                foreach (var transition in state.Transitions)
                {
                    Set(state.Name, transition.Message, transition.TargetState);
                }
            }
        }

        #endregion //File IO
    }
}
