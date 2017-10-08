using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace StateMachineBuddy
{
	/// <summary>
	/// Stores all the states, messages, and state transitions
	/// </summary>
	public class StateMachine
	{
		#region Members

		/// <summary>
		/// Event raised when current state changes
		/// </summary>
		public event EventHandler<StateChangeEventArgs> StateChangedEvent;

		/// <summary>
		/// Event that is raised when the state machine is reset to the initial state
		/// </summary>
		public event EventHandler<StateChangeEventArgs> ResetEvent;

		/// <summary>
		/// The state this machine starts in
		/// </summary>
		private int _initialState;

		/// <summary>
		/// list of the state names
		/// </summary>
		private string[] _stateNames;

		/// <summary>
		/// list of the message names
		/// </summary>
		private string[] _messageNames;

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
		/// the message offset of this state machine. 
		/// this is used so multiple state machines can use the same input queue.
		/// </summary>
		public int MessageOffset { get; set; }

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
				return _stateNames != null ? _stateNames.Length : 0;
			}
		}

		/// <summary>
		/// number of messages in this state machine
		/// </summary>
		public int NumMessages
		{
			get
			{
				return _messageNames != null ? _messageNames.Length : 0;
			}
		}

		public string CurrentStateText
		{
			get { return _stateNames[CurrentState]; }
		}

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// constructor
		/// </summary>
		public StateMachine()
		{
			_initialState = 0;
			PrevState = 0;
			CurrentState = 0;
			MessageOffset = 0;
			_stateNames = null;
			_messageNames = null;
			_data = null;
		}

		/// <summary>
		/// Set all the stuff of the state machine
		/// </summary>
		/// <param name="numStates">number of states to put in the state machine</param>
		/// <param name="numMessages">number of messages in the machine</param>
		/// <param name="initialState">the machine to start in</param>
		/// <param name="offset">the message offset for using multiple characters</param>
		public void Set(int numStates, int numMessages, int initialState = 0, int offset = 0)
		{
			//grab these variables
			_initialState = initialState;
			MessageOffset = offset;

			//allocate the data
			_data = new int[numStates, numMessages];

			//create the correct number of names for states and messages
			_stateNames = new string[numStates];
			_messageNames = new string[numMessages];

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

		public void Set(Type statesEnum, Type messagesEnum, int initialState = 0, int offset = 0)
		{
			//set the state machine up
			Set(Enum.GetValues(statesEnum).Length,
				Enum.GetValues(messagesEnum).Length,
				initialState,
				offset);

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

			//adjust the message by the offset
			int adjustedMessage = message - MessageOffset;

			Debug.Assert(state >= 0);
			Debug.Assert(state < NumStates);
			Debug.Assert(adjustedMessage >= 0);
			Debug.Assert(adjustedMessage < NumMessages);
			Debug.Assert(nextState >= 0);
			Debug.Assert(nextState < NumStates);

			//we have valid values
			_data[state, adjustedMessage] = nextState;
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
		/// Given an enum type, set the matching state names to the text of the enum
		/// </summary>
		/// <param name="states">an enum type</param>
		/// <param name="isStates">true to set state names, false to set message anmes</param>
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
			Debug.Assert(null != _stateNames);

			_stateNames[state] = stateName;
		}

		/// <summary>
		/// change the name of a message
		/// </summary>
		/// <param name="message">id of the message to change the name of</param>
		/// <param name="strMessageName">the name to change the message to</param>
		public void SetMessageName(int message, string messageName)
		{
			Debug.Assert(null != _messageNames);

			//adjust by the message offset
			int adjustedMessage = message - MessageOffset;

			Debug.Assert(adjustedMessage >= 0);
			Debug.Assert(adjustedMessage < NumMessages);

			_messageNames[adjustedMessage] = messageName;
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
					stateNames[i] = _stateNames[i];
				}
			}

			//allocate and copy new message tags
			string[] messageNames = new string[numMessages];
			for (int i = 0; i < numMessages; i++)
			{
				if (i < NumMessages)
				{
					messageNames[i] = _messageNames[i];
				}
			}

			//point to new data
			_stateNames = stateNames;
			_messageNames = messageNames;
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
			Debug.Assert(null != _stateNames);

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
					tempStateNames[curState] = _stateNames[i];

					//copy the state name
					curState++;
				}
			}

			//point to the new data
			_data = data;
			_stateNames = tempStateNames;

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
			Debug.Assert(null != _messageNames);
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
						messageNames[iCurMessage] = _messageNames[j];
						iCurMessage++;
					}
				}
			}

			//point to the new data
			_data = data;
			_messageNames = messageNames;
		}

		#endregion Initialization

		#region Important Methods

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="message">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		public virtual void SendStateMessage(int message)
		{
			Debug.Assert(null != _data);

			//change by the message offset of this dude
			int adjustedMessage = message - MessageOffset;

			Debug.Assert(adjustedMessage >= 0);
			Debug.Assert(adjustedMessage < NumMessages);

			//get the current state
			int iCurrentState = CurrentState;

			//we got a valid message
			CurrentState = _data[CurrentState, adjustedMessage];

			//did the state change
			if (iCurrentState != CurrentState)
			{
				//set the previous state
				PrevState = iCurrentState;

				//fire off a message
				OnStateChange(PrevState, CurrentState);
			}
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
				StateChangedEvent(this, new StateChangeEventArgs(oldState, nextState));
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
				ResetEvent(this, new StateChangeEventArgs(PrevState, CurrentState));
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
			Debug.Assert(null != _stateNames);

			//loop through messages to find the right one
			for (int i = 0; i < NumStates; i++)
			{
				if (stateName == _stateNames[i])
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Get the index of a message from the message name
		/// </summary>
		/// <param name="messageName">name of teh message to get the id for</param>
		/// <returns>if of the message</returns>
		public int GetMessageFromName(string messageName)
		{
			Debug.Assert(null != _messageNames);

			//loop through messages to find the right one
			for (int i = 0; i < NumMessages; i++)
			{
				if (messageName == _messageNames[i])
				{
					//adjust by the message offset
					int adjustedMessage = i + MessageOffset;
					return adjustedMessage;
				}
			}

			return -1;
		}

		/// <summary>
		/// Get a state transition
		/// </summary>
		/// <param name="state">the beginning state</param>
		/// <param name="iMessage">the message to send to that state</param>
		/// <returns>int: the id of the target state when the specified message is sent to the specified state</returns>
		public int GetEntry(int state, int message)
		{
			Debug.Assert(null != _data);

			//adjust the message by the offset
			int adjustedMessage = message - MessageOffset;

			Debug.Assert(state >= 0);
			Debug.Assert(state < NumStates);
			Debug.Assert(adjustedMessage >= 0);
			Debug.Assert(adjustedMessage < NumMessages);

			return _data[state, adjustedMessage];
		}

		/// <summary>
		/// Get the name of a state
		/// </summary>
		/// <param name="state">teh id of the state to get the name of</param>
		/// <returns>get a state name </returns>
		public string GetStateName(int state)
		{
			Debug.Assert(state >= 0);
			Debug.Assert(state < NumStates);
			Debug.Assert(null != _stateNames);

			return _stateNames[state];
		}

		/// <summary>
		/// Get the name of a message
		/// </summary>
		/// <param name="message">the id of a message to get the name of</param>
		/// <returns>given a message id, get the message name</returns>
		public string GetMessageName(int message)
		{
			Debug.Assert(null != _messageNames);

			//adjust by the message offset
			int adjustedMessage = message - MessageOffset;

			Debug.Assert(adjustedMessage >= 0);
			Debug.Assert(adjustedMessage < NumMessages);

			return _messageNames[adjustedMessage];
		}

		/// <summary>
		/// check if this is the same as another state machine
		/// todo: isn't this doing it wrong?  override isequals or whatever
		/// </summary>
		/// <param name="inst">thing to compare to</param>
		/// <returns>whether not the same thing</returns>
		public bool Compare(StateMachine inst)
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
				if (_stateNames[i] != inst._stateNames[i])
				{
					return false;
				}
			}

			for (int i = 0; i < NumMessages; i++)
			{
				if (_messageNames[i] != inst._messageNames[i])
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

		public void LoadXml(Filename file, ContentManager content)
		{
			//Load the model
			var stateMachineModel = new StateMachineModel(file);
			stateMachineModel.ReadXmlFile(content);

			//set all the state and message names
			Set(stateMachineModel.StateNames.Count, stateMachineModel.MessageNames.Count, 0, 0);
			for (int i = 0; i < stateMachineModel.StateNames.Count; i++)
			{
				_stateNames[i] = stateMachineModel.StateNames[i];
			}
			for (int i = 0; i < stateMachineModel.MessageNames.Count; i++)
			{
				_messageNames[i] = stateMachineModel.MessageNames[i];
			}

			//set the initial state
			InitialState = GetStateFromName(stateMachineModel.Initial);

			//load the state table & transitions
			LoadStateTables(stateMachineModel);
		}

		public void AppendXml(Filename file, ContentManager content)
		{
			//Load the model
			var stateMachineModel = new StateMachineModel(file);
			stateMachineModel.ReadXmlFile(content);

			//read in and append all the state & message names
			ReadNames(stateMachineModel.StateNames, stateMachineModel.MessageNames);

			//TODO: set the initial state
			//InitialState = GetStateIndexFromText(strInitialState);

			//load the state table & transitions
			LoadStateTables(stateMachineModel);
		}

		private void LoadStateTables(StateMachineModel stateMachineModel)
		{
			foreach (var stateTable in stateMachineModel.States)
			{
				LoadStateTable(stateTable);
			}
		}

		private void LoadStateTable(StateTableModel stateTableModel)
		{
			var stateIndex = GetStateFromName(stateTableModel.Name);

			//read in all the state transitions
			foreach (var stateTransition in stateTableModel.Transitions)
			{
				var message = GetMessageFromName(stateTransition.Message);
				var target = GetStateFromName(stateTransition.State);

				SetEntry(stateIndex, message, target);
			}
		}

		/// <summary>
		/// read in the state names form a file
		/// This tries to append new data into the state machine
		/// </summary>
		/// <param name="listStateNames">list of state names</param>
		/// <param name="listMessageNames">list of message names</param>
		private void ReadNames(List<string> listStateNames, List<string> listMessageNames)
		{
			//grab the current number of states & messages
			var numOldStates = NumStates;
			var numOldMessages = NumMessages;

			//How many states are new, how many are already in the state machine?
			var numNewStates = 0;
			for (var i = 0; i < listStateNames.Count; i++)
			{
				var stateIndex = GetStateFromName(listStateNames[i]);
				if (-1 == stateIndex)
				{
					numNewStates++;
				}
			}

			//how many message are new, how many are already in the state machine?
			var numNewMessages = 0;
			for (var i = 0; i < listMessageNames.Count; i++)
			{
				var messageIndex = GetMessageFromName(listMessageNames[i]);
				if (-1 == messageIndex)
				{
					numNewMessages++;
				}
			}

			//Resize the state machine to fit all the stuff currently in the state machine, as well as all the stuff from the file
			if ((numNewStates > 0) || (numNewMessages > 0))
			{
				Resize(NumStates + numNewStates, NumMessages + numNewMessages);
			}

			//read in the state names
			if (numNewStates > 0)
			{
				var nextBlankState = numOldStates;
				for (var i = 0; i < listStateNames.Count; i++)
				{
					//is this state already in there?
					var stateIndex = GetStateFromName(listStateNames[i]);
					if (-1 == stateIndex)
					{
						_stateNames[nextBlankState] = listStateNames[i];
						nextBlankState++;
					}
				}
			}

			//read in the message names
			if (numNewMessages > 0)
			{
				var nextBlankMessage = numOldMessages;
				for (var i = 0; i < listMessageNames.Count; i++)
				{
					//is this Message already in there?
					var messageIndex = GetMessageFromName(listMessageNames[i]);
					if (-1 == messageIndex)
					{
						_messageNames[nextBlankMessage] = listMessageNames[i];
						nextBlankMessage++;
					}
				}
			}
		}

#if !WINDOWS_UWP
		public void WriteXml(Filename file)
		{
			var model = new StateMachineModel(file, this);
			model.WriteXml();
		}
#endif

		#endregion //File IO
	}
}