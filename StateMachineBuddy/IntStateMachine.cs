using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StateMachineBuddy
{
    /// <summary>
    /// Stores all the states, messages, and state transitions
    /// This is a much more optimized version of the state machine
    /// </summary>
    public class IntStateMachine
    {
        #region Members

        /// <summary>
        /// Event raised when current state changes
        /// </summary>
        public event EventHandler<StateChangeEventArgs<int>> StateChangedEvent;

        /// <summary>
        /// Event that is raised when the state machine is reset to the initial state
        /// </summary>
        public event EventHandler<StateChangeEventArgs<int>> ResetEvent;

        /// <summary>
        /// The state this machine starts in
        /// </summary>
        private int _initialState;

        /// <summary>
        /// list of the state names
        /// </summary>
        public string[] StateNames { get; private set; }

        /// <summary>
        /// list of the message names
        /// </summary>
        public string[] MessageNames { get; private set; }

        /// <summary>
        /// All the state transitions for each state
        /// </summary>
        private int[,] _data;

        #endregion //Members

        #region Properties

        /// <summary>
        /// Get or set the initial state
        /// </summary>
        public int InitialState
        {
            get { return _initialState; }
            set
            {
                if ((value >= 0) && (value < NumStates))
                {
                    _initialState = value;
                }
                else
                {
                    _initialState = 0;
                }
            }
        }

        /// <summary>
        /// Gets the name of the initial state.
        /// </summary>
        public string InitialStateName => GetStateName(InitialState);

        /// <summary>
        /// The current state of this machine
        /// </summary>
        public int CurrentState { get; private set; }

        /// <summary>
        /// The last state of the machine, before we changed to the current one
        /// </summary>
        public int PrevState { get; private set; }

        /// <summary>
        /// number of states in this state machine
        /// </summary>
        public int NumStates
        {
            get
            {
                return StateNames != null ? StateNames.Length : 0;
            }
        }

        /// <summary>
        /// number of messages in this state machine
        /// </summary>
        public int NumMessages
        {
            get
            {
                return MessageNames != null ? MessageNames.Length : 0;
            }
        }

        /// <summary>
        /// Gets the name of the current state.
        /// </summary>
        public string CurrentStateName
        {
            get { return StateNames[CurrentState]; }
        }

        #endregion //Properties

        #region Initialization

        /// <summary>
        /// constructor
        /// </summary>
        public IntStateMachine()
        {
            _initialState = 0;
            PrevState = 0;
            CurrentState = 0;
            StateNames = null;
            MessageNames = null;
            _data = null;
        }

        /// <summary>
        /// Initializes the state machine with the specified number of states and messages.
        /// </summary>
        /// <param name="numStates">The number of states in the state machine.</param>
        /// <param name="numMessages">The number of messages in the state machine.</param>
        /// <param name="initialState">The initial state index.</param>
        public void Set(int numStates, int numMessages, int initialState = 0)
        {
            //grab these variables
            _initialState = initialState;

            //allocate the data
            _data = new int[numStates, numMessages];

            //create the correct number of names for states and messages
            StateNames = new string[numStates];
            MessageNames = new string[numMessages];

            //set the state transitions to defualt
            for (int i = 0; i < numStates; i++)
            {
                for (int j = 0; j < numMessages; j++)
                {
                    _data[i, j] = i;
                }
            }

            //Set the initial states
            PrevState = _initialState;
            CurrentState = _initialState;
        }

        /// <summary>
        /// Initializes the state machine using enum types for states and messages.
        /// </summary>
        /// <param name="statesEnum">The enum type defining the states.</param>
        /// <param name="messagesEnum">The enum type defining the messages.</param>
        /// <param name="initialState">The initial state index.</param>
        public void Set(Type statesEnum, Type messagesEnum, int initialState = 0)
        {
            //set the state machine up
            Set(Enum.GetValues(statesEnum).Length,
                Enum.GetValues(messagesEnum).Length,
                initialState);

            //set the names up
            SetNames(statesEnum, true);
            SetNames(messagesEnum, false);
        }

        /// <summary>
        /// Sets an state in the state table to respond to a particular message
        /// </summary>
        /// <param name="state">State to set up a message for</param>
        /// <param name="message">message to parse</param>
        /// <param name="nextState">state this state will change to after getting the message</param>
        public void SetEntry(int state, int message, int nextState)
        {
            Debug.Assert(null != _data);
            Debug.Assert(state >= 0);
            Debug.Assert(state < NumStates);
            Debug.Assert(nextState >= 0);
            Debug.Assert(nextState < NumStates);

            //we have valid values
            _data[state, message] = nextState;
        }

        /// <summary>
        /// Sets an state in the state table to respond to a particular message
        /// This is less efficeient than the other SetEntry method, so don't overuse it!
        /// </summary>
        /// <param name="state">State to set up a message for</param>
        /// <param name="messageName">name of the message to parse</param>
        /// <param name="nextStateName">name of the new state this state will change to after getting the message</param>
        public void SetEntry(int state, string messageName, string nextStateName)
        {
            //Get the state indexes from the names
            int message = GetMessageFromName(messageName);
            int nextMessage = GetStateFromName(nextStateName);
            SetEntry(state, message, nextMessage);
        }

        /// <summary>
        /// Sets state or message names from an enum type.
        /// </summary>
        /// <param name="states">The enum type to get names from.</param>
        /// <param name="isStates">True to set state names, false to set message names.</param>
        public void SetNames(Type states, bool isStates)
        {
            //get the names and values
            var names = Enum.GetNames(states);
            var values = Enum.GetValues(states);

            Debug.Assert(names.Length == values.Length);

            //loop through and set the names
            int i = 0;
            foreach (var value in values)
            {
                //Set the name of the state at that index
                if (isStates)
                {
                    SetStateName((int)value, names[i]);
                }
                else
                {
                    SetMessageName((int)value, names[i]);
                }

                i++;
            }
        }

        /// <summary>
        /// change the name of a state
        /// </summary>
        /// <param name="state">the state to change</param>
        /// <param name="stateName">the name to change the state to</param>
        public void SetStateName(int state, string stateName)
        {
            Debug.Assert(state >= 0);
            Debug.Assert(state < NumStates);
            Debug.Assert(null != StateNames);

            StateNames[state] = stateName;
        }

        /// <summary>
        /// Sets the name of a message.
        /// </summary>
        /// <param name="message">The index of the message to rename.</param>
        /// <param name="messageName">The new name for the message.</param>
        public void SetMessageName(int message, string messageName)
        {
            Debug.Assert(null != MessageNames);
            Debug.Assert(message >= 0);
            Debug.Assert(message < NumMessages);

            MessageNames[message] = messageName;
        }

        /// <summary>
        /// Resize the state machine, removing and appending from the end
        /// </summary>
        /// <param name="numStates">the new number of states</param>
        /// <param name="numMessages">the new number of messages</param>
        public void Resize(int numStates, int numMessages)
        {
            if ((numStates <= 0) || (numMessages <= 0))
            {
                return;
            }

            //create temp buffer to store data
            int[,] data = new int[numStates, numMessages];

            //copy old data
            for (int i = 0; i < numStates; i++)
            {
                for (int j = 0; j < numMessages; j++)
                {
                    //check if the state being copied is still in range of the new size
                    if ((i < NumStates) && (j < NumMessages) && (_data[i, j] < numStates))
                    {
                        data[i, j] = _data[i, j];
                    }
                    else
                    {
                        data[i, j] = i;
                    }
                }
            }

            //allocate and copy new state tags
            string[] stateNames = new string[numStates];
            for (int i = 0; i < numStates; i++)
            {
                if (i < NumStates)
                {
                    stateNames[i] = StateNames[i];
                }
            }

            //allocate and copy new message tags
            string[] messageNames = new string[numMessages];
            for (int i = 0; i < numMessages; i++)
            {
                if (i < NumMessages)
                {
                    messageNames[i] = MessageNames[i];
                }
            }

            //point to new data
            StateNames = stateNames;
            MessageNames = messageNames;
            _data = data;
        }

        /// <summary>
        /// Remove a state from the state machine
        /// </summary>
        /// <param name="state">index of the state to be removed</param>
        public void RemoveState(int state)
        {
            Debug.Assert(0 <= state);
            Debug.Assert(state < NumStates);
            Debug.Assert(NumStates > 0);
            Debug.Assert(null != _data);
            Debug.Assert(null != StateNames);

            if (NumStates == 1)
            {
                //can't remove the last state from the state machine
                return;
            }

            //set up a temp array for state data
            int[,] data = new int[(NumStates - 1), NumMessages];

            //set up temp array for state tags
            string[] tempStateNames = new string[(NumStates - 1)];

            //copy all the data into the new array, except for the state to delete
            int curState = 0;
            for (int i = 0; i < NumStates; i++)
            {
                if (state != curState)
                {
                    //copy the messages
                    for (int j = 0; j < NumMessages; j++)
                    {
                        //if the state transition goes to the target state, reset it
                        if (state == _data[i, j])
                        {
                            data[curState, j] = curState;
                        }
                        else
                        {
                            data[curState, j] = _data[i, j];
                        }
                    }

                    //copy teh state name
                    tempStateNames[curState] = StateNames[i];

                    //copy the state name
                    curState++;
                }
            }

            //point to the new data
            _data = data;
            StateNames = tempStateNames;

            //check if the initial state needs to change
            if ((_initialState >= NumStates) || (_initialState == state))
            {
                _initialState = 0;
            }
        }

        /// <summary>
        /// Remove a message from the state machine
        /// </summary>
        /// <param name="message">index of the message to be removed</param>
        public void RemoveMessage(int message)
        {
            Debug.Assert(null != _data);
            Debug.Assert(null != MessageNames);
            Debug.Assert(0 <= message);
            Debug.Assert(message < NumMessages);
            Debug.Assert(NumMessages > 0);

            if (NumMessages == 1)
            {
                //cant remove last message
                return;
            }

            //set up a temp array for state data
            int[,] data = new int[NumStates, (NumMessages - 1)];

            //set up temp array for message names
            string[] messageNames = new string[(NumMessages - 1)];

            //copy all the data into the new array, except for the message to delete
            for (int i = 0; i < NumStates; i++)
            {
                int iCurMessage = 0;
                for (int j = 0; j < NumMessages; j++)
                {
                    if (message != j)
                    {
                        data[i, iCurMessage] = _data[i, j];
                        messageNames[iCurMessage] = MessageNames[j];
                        iCurMessage++;
                    }
                }
            }

            //point to the new data
            _data = data;
            MessageNames = messageNames;
        }

        #endregion Initialization

        #region Important Methods

        /// <summary>
        /// Sends a message to the state machine, potentially triggering a state transition.
        /// </summary>
        /// <param name="message">The message index to send.</param>
        /// <returns>True if the state changed, false otherwise.</returns>
        public virtual bool SendStateMessage(int message)
        {
            Debug.Assert(null != _data);
            Debug.Assert(message >= 0);
            Debug.Assert(message < NumMessages);

            //get the current state
            int iCurrentState = CurrentState;

            //we got a valid message
            CurrentState = _data[CurrentState, message];

            //did the state change
            if (iCurrentState != CurrentState)
            {
                //set the previous state
                PrevState = iCurrentState;

                //fire off a message
                OnStateChange(PrevState, CurrentState);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to force the state machine to a certain state
        /// </summary>
        /// <param name="state">state to set machine to</param>
        public void ForceState(int state)
        {
            Debug.Assert(state >= 0);
            Debug.Assert(state < NumStates);

            int currentState = CurrentState;
            CurrentState = state;

            //did the state change
            if (currentState != CurrentState)
            {
                //set the previous state
                PrevState = currentState;

                //fire off a message
                OnStateChange(PrevState, CurrentState);
            }
        }

        /// <summary>
        /// Method for raising the state change event.
        /// </summary>
        /// <param name="oldState">the state changing from</param>
        /// <param name="nextState">the next state</param>
        protected virtual void OnStateChange(int oldState, int nextState)
        {
            if (StateChangedEvent != null)
            {
                StateChangedEvent(this, new StateChangeEventArgs<int>(oldState, nextState));
            }
        }

        /// <summary>
        /// This function sets the thing to its initial state.
        /// </summary>
        public void ResetToInitialState()
        {
            PrevState = _initialState;
            CurrentState = _initialState;

            //Send the reset event instread of the state change event
            if (ResetEvent != null)
            {
                ResetEvent(this, new StateChangeEventArgs<int>(PrevState, CurrentState));
            }
        }

        #endregion //Important Methods

        #region Access Methods

        /// <summary>
        /// Get the index of a state based on a state name
        /// </summary>
        /// <param name="stateName">name of the state to get the index of</param>
        /// <returns>int index of the state with that name, -1 if no state found</returns>
        public int GetStateFromName(string stateName)
        {
            Debug.Assert(null != StateNames);

            //loop through messages to find the right one
            for (int i = 0; i < NumStates; i++)
            {
                if (stateName == StateNames[i])
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of a message from its name.
        /// </summary>
        /// <param name="messageName">The name of the message.</param>
        /// <returns>The index of the message, or -1 if not found.</returns>
        public int GetMessageFromName(string messageName)
        {
            Debug.Assert(null != MessageNames);

            //loop through messages to find the right one
            for (int i = 0; i < NumMessages; i++)
            {
                if (messageName == MessageNames[i])
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the target state for a given state and message combination.
        /// </summary>
        /// <param name="state">The source state index.</param>
        /// <param name="message">The message index.</param>
        /// <returns>The target state index for the transition.</returns>
        public int GetEntry(int state, int message)
        {
            Debug.Assert(null != _data);
            Debug.Assert(state >= 0);
            Debug.Assert(state < NumStates);
            Debug.Assert(message >= 0);
            Debug.Assert(message < NumMessages);

            return _data[state, message];
        }

        /// <summary>
        /// Gets the name of a state.
        /// </summary>
        /// <param name="state">The state index.</param>
        /// <returns>The name of the state.</returns>
        public string GetStateName(int state)
        {
            Debug.Assert(state >= 0);
            Debug.Assert(state < NumStates);
            Debug.Assert(null != StateNames);

            return StateNames[state];
        }

        /// <summary>
        /// Get the name of a message
        /// </summary>
        /// <param name="message">the id of a message to get the name of</param>
        /// <returns>given a message id, get the message name</returns>
        public string GetMessageName(int message)
        {
            Debug.Assert(null != MessageNames);
            Debug.Assert(message >= 0);
            Debug.Assert(message < NumMessages);

            return MessageNames[message];
        }

        /// <summary>
        /// Compares this state machine with another for equality.
        /// </summary>
        /// <param name="inst">The state machine to compare with.</param>
        /// <returns>True if the state machines are equal, false otherwise.</returns>
        public bool Compare(IntStateMachine inst)
        {
            //compare two state machines
            if ((_initialState != inst._initialState) ||
                (NumStates != inst.NumStates) ||
                (NumMessages != inst.NumMessages))
            {
                return false;
            }

            for (int i = 0; i < NumStates; i++)
            {
                if (StateNames[i] != inst.StateNames[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < NumMessages; i++)
            {
                if (MessageNames[i] != inst.MessageNames[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < NumStates; i++)
            {
                for (int j = 0; j < NumMessages; j++)
                {
                    if (_data[i, j] != inst._data[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion //Access Methods

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
        private void LoadStateMachine(StateMachineModel stateMachineModel)
        {
            //set all the state and message names
            Set(stateMachineModel.StateNames.Count, stateMachineModel.MessageNames.Count, 0);
            for (int i = 0; i < stateMachineModel.StateNames.Count; i++)
            {
                StateNames[i] = stateMachineModel.StateNames[i];
            }
            for (int i = 0; i < stateMachineModel.MessageNames.Count; i++)
            {
                MessageNames[i] = stateMachineModel.MessageNames[i];
            }

            //set the initial state
            InitialState = GetStateFromName(stateMachineModel.Initial);

            //load the state table & transitions
            LoadStateTables(stateMachineModel);
        }

        /// <summary>
        /// Loads all state tables from the model.
        /// </summary>
        /// <param name="stateMachineModel">The model containing state data.</param>
        private void LoadStateTables(StateMachineModel stateMachineModel)
        {
            foreach (var stateTable in stateMachineModel.States)
            {
                LoadStateTable(stateTable);
            }
        }

        /// <summary>
        /// Loads a single state table from a state model.
        /// </summary>
        /// <param name="stateTableModel">The state model to load.</param>
        private void LoadStateTable(StateModel stateTableModel)
        {
            var stateIndex = GetStateFromName(stateTableModel.Name);

            //read in all the state transitions
            foreach (var stateTransition in stateTableModel.Transitions)
            {
                var message = GetMessageFromName(stateTransition.Message);
                var target = GetStateFromName(stateTransition.TargetState);

                SetEntry(stateIndex, message, target);
            }
        }

        #endregion //File IO
    }
}