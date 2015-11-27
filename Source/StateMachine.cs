using FilenameBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
#if OUYA
using Ouya.Console.Api;
#endif

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
		public int MessageOffset { get; private set; }

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

		#region Methods

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
		/// Method for raising the state change event.
		/// </summary>
		protected virtual void OnStateChange(int oldState, int newState)
		{
			if (StateChangedEvent != null)
			{
				StateChangedEvent(this, new StateChangeEventArgs(oldState, newState));
			}
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

			//set the state transitions to defualt
			for (int i = 0; i < NumStates; i++)
			{
				for (int j = 0; j < NumMessages; j++)
				{
					_data[i, j] = i;
				}
			}

			//create the correct number of names for states and messages
			_stateNames = new string[numStates];
			_messageNames = new string[numMessages];

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
		/// <param name="iMessage">message to parse</param>
		/// <param name="iNewState">state this state will change to after getting the message</param>
		public void SetEntry(int state, int message, int newState)
		{
			Debug.Assert(null != _data);

			//adjust the message by the offset
			int adjustedMessage = message - MessageOffset;

			Debug.Assert(state >= 0);
			Debug.Assert(state < NumStates);
			Debug.Assert(adjustedMessage >= 0);
			Debug.Assert(adjustedMessage < NumMessages);
			Debug.Assert(newState >= 0);
			Debug.Assert(newState < NumStates);

			//we have valid values
			_data[state, adjustedMessage] = newState;
		}

		/// <summary>
		/// Sets an state in the state table to respond to a particular message
		/// This is less efficeient than the other SetEntry method, so don't overuse it!
		/// </summary>
		/// <param name="iState">State to set up a message for</param>
		/// <param name="strMessage">name of the message to parse</param>
		/// <param name="strNewState">name of the new state this state will change to after getting the message</param>
		public void SetEntry(int iState, string strMessage, string strNewState)
		{
			//Get the state indexes from the names
			int iMessage = GetMessageIndexFromText(strMessage);
			int iNewState = GetStateIndexFromText(strNewState);
			SetEntry(iState, iMessage, iNewState);
		}

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="iMessage">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		public virtual void SendStateMessage(int iMessage)
		{
			Debug.Assert(null != _data);

			//change by the message offset of this dude
			int iAdjustedMessage = iMessage - MessageOffset;

			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < NumMessages);

			//get the current state
			int iCurrentState = CurrentState;

			//we got a valid message
			CurrentState = _data[CurrentState, iAdjustedMessage];

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
		/// <param name="iState">state to set machine to</param>
		public void ForceState(int iState)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < NumStates);

			int iCurrentState = CurrentState;
			CurrentState = iState;

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
		/// Get the index of a state based on a state name
		/// </summary>
		/// <param name="strText">name of the state to get the index of</param>
		/// <returns>int index of the state with that name, -1 if no state found</returns>
		public int GetStateIndexFromText(string strText)
		{
			Debug.Assert(null != _stateNames);

			//loop through messages to find the right one
			for (int i = 0; i < NumStates; i++)
			{
				if (strText == _stateNames[i])
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Get the index of a message from the message name
		/// </summary>
		/// <param name="strText">name of teh message to get the id for</param>
		/// <returns>if of the message</returns>
		public int GetMessageIndexFromText(string strText)
		{
			Debug.Assert(null != _messageNames);

			//loop through messages to find the right one
			for (int i = 0; i < NumMessages; i++)
			{
				if (strText == _messageNames[i])
				{
					//adjust by the message offset
					int iAdjustedMessage = i + MessageOffset;
					return iAdjustedMessage;
				}
			}

			return -1;
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

		/// <summary>
		/// Resize the state machine, removing and appending from the end
		/// </summary>
		/// <param name="iNumStates">the new number of states</param>
		/// <param name="iNumMessages">the new number of messages</param>
		public void Resize(int iNumStates, int iNumMessages)
		{
			if ((iNumStates <= 0) || (iNumMessages <= 0))
			{
				return;
			}

			//create temp buffer to store data
			int[,] pData = new int[iNumStates, iNumMessages];

			//copy old data
			for (int i = 0; i < iNumStates; i++)
			{
				for (int j = 0; j < iNumMessages; j++)
				{
					//check if the state being copied is still in range of the new size
					if ((i < NumStates) && (j < NumMessages) && (_data[i, j] < iNumStates))
					{
						pData[i, j] = _data[i, j];
					}
					else
					{
						pData[i, j] = i;
					}
				}
			}

			//allocate and copy new state tags
			string[] StateNames = new string[iNumStates];
			for (int i = 0; i < iNumStates; i++)
			{
				if (i < NumStates)
				{
					StateNames[i] = _stateNames[i];
				}
			}

			//allocate and copy new message tags
			string[] MessageNames = new string[iNumMessages];
			for (int i = 0; i < iNumMessages; i++)
			{
				if (i < NumMessages)
				{
					MessageNames[i] = _messageNames[i];
				}
			}

			//point to new data
			_stateNames = StateNames;
			_messageNames = MessageNames;
			_data = pData;
		}

		/// <summary>
		/// Get a state transition
		/// </summary>
		/// <param name="iState">the beginning state</param>
		/// <param name="iMessage">the message to send to that state</param>
		/// <returns>int: the id of the target state when the specified message is sent to the specified state</returns>
		public int GetEntry(int iState, int iMessage)
		{
			Debug.Assert(null != _data);

			//adjust the message by the offset
			int iAdjustedMessage = iMessage - MessageOffset;

			Debug.Assert(iState >= 0);
			Debug.Assert(iState < NumStates);
			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < NumMessages);

			return _data[iState, iAdjustedMessage];
		}

		/// <summary>
		/// Get the name of a state
		/// </summary>
		/// <param name="iState">teh id of the state to get the name of</param>
		/// <returns>get a state name </returns>
		public string GetStateName(int iState)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < NumStates);
			Debug.Assert(null != _stateNames);

			return _stateNames[iState];
		}

		/// <summary>
		/// Get the name of a message
		/// </summary>
		/// <param name="iMessage">the id of a message to get the name of</param>
		/// <returns>given a message id, get the message name</returns>
		public string GetMessageName(int iMessage)
		{
			Debug.Assert(null != _messageNames);

			//adjust by the message offset
			int iAdjustedMessage = iMessage - MessageOffset;

			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < NumMessages);

			return _messageNames[iAdjustedMessage];
		}

		/// <summary>
		/// change the name of a state
		/// </summary>
		/// <param name="iState">the state to change</param>
		/// <param name="strStateName">the name to change the state to</param>
		public void SetStateName(int iState, string strStateName)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < NumStates);
			Debug.Assert(null != _stateNames);

			_stateNames[iState] = strStateName;
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
				//Get the state index
				int index = (int) value;

				//get teh name
				string name = names[i];

				//Set the name of the state at that index
				if (isStates)
				{
					SetStateName(index, name);
				}
				else
				{
					SetMessageName(index, name);
				}

				i++;
			}
		}

		/// <summary>
		/// change the name of a message
		/// </summary>
		/// <param name="iMessage">id of the message to change the name of</param>
		/// <param name="strMessageName">the name to change the message to</param>
		public void SetMessageName(int iMessage, string strMessageName)
		{
			Debug.Assert(null != _messageNames);

			//adjust by the message offset
			int iAdjustedMessage = iMessage - MessageOffset;

			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < NumMessages);

			_messageNames[iAdjustedMessage] = strMessageName;
		}

		/// <summary>
		/// Remove a state from the state machine
		/// </summary>
		/// <param name="iState">index of the state to be removed</param>
		public void RemoveState(int iState)
		{
			Debug.Assert(0 <= iState);
			Debug.Assert(iState < NumStates);
			Debug.Assert(NumStates > 0);
			Debug.Assert(null != _data);
			Debug.Assert(null != _stateNames);

			if (NumStates == 1)
			{
				//can't remove the last state from the state machine
				return;
			}

			//set up a temp array for state data
			int[,] pData = new int[(NumStates - 1), NumMessages];

			//set up temp array for state tags
			string[] pTempStrings = new string[(NumStates - 1)];

			//copy all the data into the new array, except for the state to delete
			int iCurState = 0;
			for (int i = 0; i < NumStates; i++)
			{
				if (iState != iCurState)
				{
					//copy the messages
					for (int j = 0; j < NumMessages; j++)
					{
						//if the state transition goes to the target state, reset it
						if (iState == _data[i, j])
						{
							pData[iCurState, j] = iCurState;
						}
						else
						{
							pData[iCurState, j] = _data[i, j];
						}
					}

					//copy teh state name
					pTempStrings[iCurState] = _stateNames[i];

					//copy the state name
					iCurState++;
				}
			}

			//point to the new data
			_data = pData;
			_stateNames = pTempStrings;

			//check if the initial state needs to change
			if ((_initialState >= NumStates) || (_initialState == iState))
			{
				_initialState = 0;
			}
		}

		/// <summary>
		/// Remove a message from the state machine
		/// </summary>
		/// <param name="iMessage">index of the message to be removed</param>
		public void RemoveMessage(int iMessage)
		{
			Debug.Assert(null != _data);
			Debug.Assert(null != _messageNames);
			Debug.Assert(0 <= iMessage);
			Debug.Assert(iMessage < NumMessages);
			Debug.Assert(NumMessages > 0);

			if (NumMessages == 1)
			{
				//cant remove last message
				return;
			}

			//set up a temp array for state data
			int[,] pData = new int[NumStates, (NumMessages - 1)];

			//set up temp array for message names
			string[] pTempStrings = new string[(NumMessages - 1)];

			//copy all the data into the new array, except for the message to delete
			for (int i = 0; i < NumStates; i++)
			{
				int iCurMessage = 0;
				for (int j = 0; j < NumMessages; j++)
				{
					if (iMessage != j)
					{
						pData[i, iCurMessage] = _data[i, j];
						pTempStrings[iCurMessage] = _messageNames[j];
						iCurMessage++;
					}
				}
			}

			//point to the new data
			_data = pData;
			_messageNames = pTempStrings;
		}

		/// <summary>
		/// check if this is the same as another state machine
		/// todo: isn't this doing it wrong?  override isequals or whatever
		/// </summary>
		/// <param name="rInst">thing to compare to</param>
		/// <returns>whether not the same thing</returns>
		public bool Compare(StateMachine rInst)
		{
			//compare two state machines
			if ((_initialState != rInst._initialState) ||
				(NumStates != rInst.NumStates) ||
				(NumMessages != rInst.NumMessages))
			{
				return false;
			}

			for (int i = 0; i < NumStates; i++)
			{
				if (_stateNames[i] != rInst._stateNames[i])
				{
					return false;
				}
			}

			for (int i = 0; i < NumMessages; i++)
			{
				if (_messageNames[i] != rInst._messageNames[i])
				{
					return false;
				}
			}

			for (int i = 0; i < NumStates; i++)
			{
				for (int j = 0; j < NumMessages; j++)
				{
					if (_data[i, j] != rInst._data[i, j])
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// read in serialized xna state machine from XML
		/// </summary>
		/// <param name="strFilename">file to open</param>
		/// <returns>whether or not it was able to open it</returns>
		public bool ReadXmlFile(Filename strFilename)
		{
			// Open the file.
			#if ANDROID
			Stream stream = Game.Activity.Assets.Open(strFilename.File);
			#else
			FileStream stream = File.Open(strFilename.File, FileMode.Open, FileAccess.Read);
			#endif
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			XmlNode rootNode = xmlDoc.DocumentElement;

			if (rootNode.NodeType != XmlNodeType.Element)
			{
				//should be an xml node!!!
				Debug.Assert(false);
				return false;
			}

			//eat up the name of that xml node
			if (("XnaContent" != rootNode.Name) || !rootNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}

			//next node is "<Asset Type="SPFSettings.StateMachineXML">"
			XmlNode AssetNode = rootNode.FirstChild;
			if (null == AssetNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!AssetNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}
			if ("Asset" != AssetNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//next node is "<initial>"
			XmlNode InitialNode = AssetNode.FirstChild;
			if (null == InitialNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("initial" != InitialNode.Name)
			{
				Debug.Assert(false);
				return false;
			}
			string strInitialState = InitialNode.InnerXml;

			//next node is the state names
			XmlNode StateNamesNode = InitialNode.NextSibling;
			if (null == StateNamesNode)
			{
				Debug.Assert(false);
				return false;
			}

			if ("stateNames" != StateNamesNode.Name)
			{
				Debug.Assert(false);
				return false;
			}
			
			//read in all the state names
			List<string> listStateNames = new List<string>();
			for (XmlNode childNode = StateNamesNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
				listStateNames.Add(childNode.InnerXml);
			}

			//next node is the message names
			XmlNode MessageNamesNode = StateNamesNode.NextSibling;
			if (null == MessageNamesNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("messageNames" != MessageNamesNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//read in all the message names
			List<string> listMessageNames = new List<string>();
			for (XmlNode childNode = MessageNamesNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
				listMessageNames.Add(childNode.InnerXml);
			}

			//set all the state and message names
			Set(listStateNames.Count, listMessageNames.Count, 0, 0);
			for (int i = 0; i < listStateNames.Count; i++)
			{
				_stateNames[i] = listStateNames[i];
			}
			for (int i = 0; i < listMessageNames.Count; i++)
			{
				_messageNames[i] = listMessageNames[i];
			}

			//set the initial state
			InitialState = GetStateIndexFromText(strInitialState);
			Debug.Assert(-1 != InitialState);

			//next node is the states
			XmlNode StatesNode = MessageNamesNode.NextSibling;
			if (null == StatesNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("states" != StatesNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//the rest of the nodes are the states
			for (XmlNode StateTableNode = StatesNode.FirstChild;
				null != StateTableNode;
				StateTableNode = StateTableNode.NextSibling)
			{
				if (!ReadXmlStateTable(StateTableNode))
				{
					Debug.Assert(false);
					return false;
				}
			}

			// Close the file.
			stream.Close();
			return true;
		}

		/// <summary>
		/// read in serialized xna state machine from XML
		/// </summary>
		/// <param name="strFilename">file to open</param>
		/// <returns>whether or not it was able to open it</returns>
		public bool AppendXmlFile(Filename strFilename)
		{
			// Open the file.
			#if ANDROID
			Stream stream = Game.Activity.Assets.Open(strFilename.File);
			#else
			FileStream stream = File.Open(strFilename.File, FileMode.Open, FileAccess.Read);
			#endif
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(stream);
			XmlNode rootNode = xmlDoc.DocumentElement;

			if (rootNode.NodeType != XmlNodeType.Element)
			{
				//should be an xml node!!!
				Debug.Assert(false);
				return false;
			}

			//eat up the name of that xml node
			if (("XnaContent" != rootNode.Name) || !rootNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}

			//next node is "<Asset Type="SPFSettings.StateMachineXML">"
			XmlNode AssetNode = rootNode.FirstChild;
			if (null == AssetNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!AssetNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}
			if ("Asset" != AssetNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//next node is "<initial>"
			XmlNode InitialNode = AssetNode.FirstChild;
			if (null == InitialNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("initial" != InitialNode.Name)
			{
				Debug.Assert(false);
				return false;
			}
			
			string strInitialState = InitialNode.InnerXml;

			//next node is the state names
			XmlNode StateNamesNode = InitialNode.NextSibling;
			if (null == StateNamesNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("stateNames" != StateNamesNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//read in all the state names
			List<string> listStateNames = new List<string>();
			for (XmlNode childNode = StateNamesNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
				listStateNames.Add(childNode.InnerXml);
			}

			//next node is the message names
			XmlNode MessageNamesNode = StateNamesNode.NextSibling;
			if (null == MessageNamesNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("messageNames" != MessageNamesNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//read in all the message names
			List<string> listMessageNames = new List<string>();
			for (XmlNode childNode = MessageNamesNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
				listMessageNames.Add(childNode.InnerXml);
			}

			//read in and append all the state & message names
			ReadNames(listStateNames, listMessageNames);

			//TODO: set the initial state
			//InitialState = GetStateIndexFromText(strInitialState);
			Debug.Assert(-1 != InitialState);

			//next node is the states
			XmlNode StatesNode = MessageNamesNode.NextSibling;
			if (null == StatesNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("states" != StatesNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//the rest of the nodes are the states
			for (XmlNode StateTableNode = StatesNode.FirstChild;
				null != StateTableNode;
				StateTableNode = StateTableNode.NextSibling)
			{
				if (!ReadXmlStateTable(StateTableNode))
				{
					Debug.Assert(false);
					return false;
				}
			}

			// Close the file.
			stream.Close();
			return true;
		}

		private bool ReadXmlStateTable(XmlNode StateTableNode)
		{
			//this is the <Item Type="SPFSettings.StateTableXML"> node
			if (null == StateTableNode)
			{
				Debug.Assert(false);
				return false;
			}
			if (!StateTableNode.HasChildNodes)
			{
				Debug.Assert(false);
				return false;
			}
			if ("Item" != StateTableNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//first child is the name of this state
			XmlNode CurrentStateNameNode = StateTableNode.FirstChild;
			if (null == CurrentStateNameNode)
			{
				Debug.Assert(false);
				return false;
			}
			if ("name" != CurrentStateNameNode.Name)
			{
				Debug.Assert(false);
				return false;
			}
			string strCurrentStateName = CurrentStateNameNode.InnerXml;
			int iCurrentState = GetStateIndexFromText(strCurrentStateName);

			Debug.Assert(-1 != iCurrentState);

			//next item is <transitions>
			XmlNode TransitionsNode = CurrentStateNameNode.NextSibling;
			if (null == TransitionsNode)
			{
				Debug.Assert(false);
				return false;
			}

			if ("transitions" != TransitionsNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//read in all the state transitions
			for (XmlNode StateChangeNode = TransitionsNode.FirstChild;
				null != StateChangeNode;
				StateChangeNode = StateChangeNode.NextSibling)
			{
				//next item will be <Item Type="SPFSettings.StateChangeXML">
				if (null == StateChangeNode)
				{
					Debug.Assert(false);
					return false;
				}
				if (!StateChangeNode.HasChildNodes)
				{
					Debug.Assert(false);
					return false;
				}
				if ("Item" != StateChangeNode.Name)
				{
					Debug.Assert(false);
					return false;
				}

				//read in the next two nodes
				XmlNode StateChangeMessageNode = StateChangeNode.FirstChild;
				if (null == StateChangeMessageNode)
				{
					Debug.Assert(false);
					return false;
				}
				if ("message" != StateChangeMessageNode.Name)
				{
					Debug.Assert(false);
					return false;
				}

				XmlNode StateChangeTargetNode = StateChangeMessageNode.NextSibling;
				if (null == StateChangeTargetNode)
				{
					Debug.Assert(false);
					return false;
				}
				if ("state" != StateChangeTargetNode.Name)
				{
					Debug.Assert(false);
					return false;
				}

				string strMessage = StateChangeMessageNode.InnerXml;
				string strTargetState = StateChangeTargetNode.InnerXml;

				int iMessage = GetMessageIndexFromText(StateChangeMessageNode.InnerXml);
				int iTargetState = GetStateIndexFromText(StateChangeTargetNode.InnerXml);

				Debug.Assert(iMessage >= 0);
				Debug.Assert(iTargetState >= 0);
				SetEntry(iCurrentState, iMessage, iTargetState);
			}

			return true;
		}

		/// <summary>
		/// write out serialized xna state machine as XML
		/// </summary>
		/// <param name="strFilename">teh file to write out to</param>
		public void WriteXml(Filename strFilename)
		{
			//open the file, create it if it doesnt exist yet
			XmlTextWriter rXMLFile = new XmlTextWriter(strFilename.File, null);
			rXMLFile.Formatting = Formatting.Indented;
			rXMLFile.Indentation = 1;
			rXMLFile.IndentChar = '\t';

			rXMLFile.WriteStartDocument();

			//add the xml node
			rXMLFile.WriteStartElement("XnaContent");
			rXMLFile.WriteStartElement("Asset");
			rXMLFile.WriteAttributeString("Type", "SPFSettings.StateMachineXML");

			//write out the initial state
			rXMLFile.WriteStartElement("initial");
			rXMLFile.WriteString(GetStateName(_initialState));
			rXMLFile.WriteEndElement();

			//write out the state names
			rXMLFile.WriteStartElement("stateNames");
			for (int i = 0; i < NumStates; i++)
			{
				rXMLFile.WriteStartElement("Item");
				rXMLFile.WriteAttributeString("Type", "string");
				rXMLFile.WriteString(_stateNames[i]);
				rXMLFile.WriteEndElement();
			}
			rXMLFile.WriteEndElement();

			//write out the message names
			rXMLFile.WriteStartElement("messageNames");
			for (int i = 0; i < NumMessages; i++)
			{
				rXMLFile.WriteStartElement("Item");
				rXMLFile.WriteAttributeString("Type", "string");
				rXMLFile.WriteString(_messageNames[i]);
				rXMLFile.WriteEndElement();
			}
			rXMLFile.WriteEndElement();

			//write out all the data
			rXMLFile.WriteStartElement("states");
			for (int i = 0; i < NumStates; i++)
			{
				//write out one state table for each state
				rXMLFile.WriteStartElement("Item");
				rXMLFile.WriteAttributeString("Type", "SPFSettings.StateTableXML");

				//write out the name of the state
				rXMLFile.WriteStartElement("name");
				rXMLFile.WriteString(_stateNames[i]);
				rXMLFile.WriteEndElement();

				//write out all teh state transitions
				rXMLFile.WriteStartElement("transitions");

				for (int j = 0; j < NumMessages; j++)
				{
					//Comment this if check out if you want to write out allll state transitions
					if (_data[i, j] != i)
					{
						rXMLFile.WriteStartElement("Item");
						rXMLFile.WriteAttributeString("Type", "SPFSettings.StateChangeXML");

						rXMLFile.WriteStartElement("message");
						rXMLFile.WriteString(_messageNames[j]);
						rXMLFile.WriteEndElement();

						rXMLFile.WriteStartElement("state");
						rXMLFile.WriteString(GetStateName(_data[i, j]));
						rXMLFile.WriteEndElement();

						rXMLFile.WriteEndElement();
					}
				}

				rXMLFile.WriteEndElement();
				rXMLFile.WriteEndElement();
			}
			rXMLFile.WriteEndElement();

			rXMLFile.WriteEndElement();
			rXMLFile.WriteEndElement();

			rXMLFile.WriteEndDocument();

			// Close the file.
			rXMLFile.Flush();
			rXMLFile.Close();
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
			int iNumOldStates = NumStates;
			int iNumOldMessages = NumMessages;

			//How many states are new, how many are already in the state machine?
			int iNumNewStates = 0;
			for (int i = 0; i < listStateNames.Count; i++)
			{
				int iStateIndex = GetStateIndexFromText(listStateNames[i]);
				if (-1 == iStateIndex)
				{
					iNumNewStates++;
				}
			}

			//how many message are new, how many are already in the state machine?
			int iNumNewMessages = 0;
			for (int i = 0; i < listMessageNames.Count; i++)
			{
				int iMessageIndex = GetMessageIndexFromText(listMessageNames[i]);
				if (-1 == iMessageIndex)
				{
					iNumNewMessages++;
				}
			}

			//Resize the state machine to fit all the stuff currently in the state machine, as well as all the stuff from the file
			if ((iNumNewStates > 0) || (iNumNewMessages > 0))
			{
				Resize(NumStates + iNumNewStates, NumMessages + iNumNewMessages);
			}

			//read in the state names
			if (iNumNewStates > 0)
			{
				int iNextBlankState = iNumOldStates;
				for (int i = 0; i < listStateNames.Count; i++)
				{
					//is this state already in there?
					int iStateIndex = GetStateIndexFromText(listStateNames[i]);
					if (-1 == iStateIndex)
					{
						_stateNames[iNextBlankState] = listStateNames[i];
						iNextBlankState++;
					}
				}
			}

			//read in the message names
			if (iNumNewMessages > 0)
			{
				int iNextBlankMessage = iNumOldMessages;
				for (int i = 0; i < listMessageNames.Count; i++)
				{
					//is this Message already in there?
					int iMessageIndex = GetMessageIndexFromText(listMessageNames[i]);
					if (-1 == iMessageIndex)
					{
						_messageNames[iNextBlankMessage] = listMessageNames[i];
						iNextBlankMessage++;
					}
				}
			}
		}

		#endregion //File IO
	}
}