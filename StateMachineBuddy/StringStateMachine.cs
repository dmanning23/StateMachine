using System;
using System.Collections.Generic;
using System.Linq;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace StateMachineBuddy
{
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

        public StringStateMachine()
        {
        }

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
        /// Set up the initial state of this thing
        /// </summary>
        /// <param name="initialState"></param>
        public void SetInitialState(string initialState)
        {
            if (States.Contains(initialState))
            {
                InitialState = initialState;
                PrevState = initialState;
                CurrentState = initialState;
            }
        }

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

        public void AddStateMachine(Type statesEnum, Type messagesEnum, string initialState)
        {
            AddStates(statesEnum);
            AddMessages(messagesEnum);
            AddMessages(Enum.GetNames(messagesEnum).ToList());

            SetInitialState(initialState);
        }

        public void AddStates(Type statesEnum)
        {
            AddStates(Enum.GetNames(statesEnum).ToList());
        }

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
        /// method to send a message
        /// </summary>
        /// <param name="message">message to send to the state machine, 
        /// should be offset by the message offset of this dude</param>
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

        public void LoadXml(Filename file, ContentManager xmlContent)
        {
            //Load the model
            using (var stateMachineModel = new StateMachineModel(file))
            {
                stateMachineModel.ReadXmlFile(xmlContent);

                LoadStateMachine(stateMachineModel);
            }
        }

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
