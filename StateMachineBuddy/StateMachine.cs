using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Net;

namespace StateMachineBuddy
{
	/// <summary>
	/// Stores all the states, messages, and state transitions
	/// </summary>
	public class StateMachine
	{
		#region Members

		/// <summary>
		/// The state this machine starts in
		/// </summary>
		int m_iInitialState;

		/// <summary>
		/// The current state of this machine
		/// </summary>
		int m_iCurrentState;

		/// <summary>
		/// The last state of the machine, before we changed to the current one
		/// </summary>
		int m_iPrevState;

		/// <summary>
		/// the message offset of this state machine. 
		/// this is used so multiple state machines can use the same input queue.
		/// </summary>
		int m_iMessageOffset;

		/// <summary>
		/// number of states in this state machine
		/// </summary>
		int m_iNumStates;

		/// <summary>
		/// number of messages in this state machine
		/// </summary>
		int m_iNumMessages;

		/// <summary>
		/// list of the state names
		/// </summary>
		string[] m_listStateNames;

		/// <summary>
		/// list of the message names
		/// </summary>
		string[] m_listMessageNames;

		/// <summary>
		/// All the state transitions for each state
		/// </summary>
		int[,] m_Data;

		#endregion //Members

		#region Properties

		/// <summary>
		/// Get or set the initial state
		/// </summary>
		public int InitialState
		{
			get { return m_iInitialState; }
			set
			{
				if ((value >= 0) && (value < m_iNumMessages))
				{
					m_iInitialState = value;
				}
				else
				{
					m_iInitialState = 0;
				}
			}
		}

		public int MessageOffset
		{
			get { return m_iMessageOffset; }
		}

		public int CurrentState
		{
			get { return m_iCurrentState; }
		}

		public int PrevState
		{
			get { return m_iPrevState; }
		}

		public int NumStates
		{
			get { return m_iNumStates; }
		}

		public int NumMessages
		{
			get { return m_iNumMessages; }
		}

		public string CurrentStateText
		{
			get { return m_listStateNames[CurrentState]; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// constructor
		/// </summary>
		public StateMachine()
		{
			m_iInitialState = 0;
			m_iPrevState = 0;
			m_iCurrentState = 0;
			m_iMessageOffset = 0;
			m_iNumStates = 0;
			m_iNumMessages = 0;
			m_listStateNames = null;
			m_listMessageNames = null;
			m_Data = null;
		}

		/// <summary>
		/// Set all the stuff of the state machine
		/// </summary>
		/// <param name="iNumStates">number of states to put in the state machine</param>
		/// <param name="iNumMessages">number of messages in the machine</param>
		/// <param name="iInitialState">the machine to start in</param>
		/// <param name="iOffset">the message offset for using multiple characters</param>
		public void Set(int iNumStates, int iNumMessages, int iInitialState, int iOffset)
		{
			//grab these variables
			m_iInitialState = iInitialState;
			m_iMessageOffset = iOffset;
			m_iNumStates = iNumStates;
			m_iNumMessages = iNumMessages;

			//allocate the data
			m_Data = new int[m_iNumStates, m_iNumMessages];

			//set the state transitions to defualt
			for (int i = 0; i < m_iNumStates; i++)
			{
				for (int j = 0; j < m_iNumMessages; j++)
				{
					m_Data[i, j] = i;
				}
			}

			//create the correct number of names for states and messages
			m_listStateNames = new string[m_iNumStates];
			m_listMessageNames = new string[m_iNumMessages];
		}

		/// <summary>
		/// Sets an state in the state table to respond to a particular message
		/// </summary>
		/// <param name="iState">State to set up a message for</param>
		/// <param name="iMessage">message to parse</param>
		/// <param name="iNewState">state this state will change to after getting the message</param>
		public void SetEntry(int iState, int iMessage, int iNewState)
		{
			Debug.Assert(null != m_Data);

			//adjust the message by the offset
			int iAdjustedMessage = iMessage - m_iMessageOffset;

			Debug.Assert(iState >= 0);
			Debug.Assert(iState < m_iNumStates);
			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < m_iNumMessages);
			Debug.Assert(iNewState >= 0);
			Debug.Assert(iNewState < m_iNumStates);

			//we have valid values
			m_Data[iState, iAdjustedMessage] = iNewState;
		}

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="iMessage">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		/// <returns>bool: did it change states?</returns>
		public virtual bool SendStateMessage(int iMessage)
		{
			Debug.Assert(null != m_Data);

			//change by the message offset of this dude
			int iAdjustedMessage = iMessage - m_iMessageOffset;

			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < m_iNumMessages);

			//get the current state
			int iCurrentState = m_iCurrentState;

			//we got a valid message
			m_iCurrentState = m_Data[m_iCurrentState, iAdjustedMessage];

			//did the state change
			if (iCurrentState != m_iCurrentState)
			{
				//set the previous state
				m_iPrevState = iCurrentState;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Method to force the state machine to a certain state
		/// </summary>
		/// <param name="iState">state to set machine to</param>
		/// <returns>bool: whether or not the state changed</returns>
		public bool ForceState(int iState)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < m_iNumStates);

			int iCurrentState = m_iCurrentState;
			m_iCurrentState = iState;

			//did the state change
			if (iCurrentState != m_iCurrentState)
			{
				//set the previous state
				m_iPrevState = iCurrentState;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Get the index of a state based on a state name
		/// </summary>
		/// <param name="strText">name of the state to get the index of</param>
		/// <returns>int index of the state with that name, -1 if no state found</returns>
		public int GetStateIndexFromText(string strText)
		{
			Debug.Assert(null != m_listStateNames);

			//loop through messages to find the right one
			for (int i = 0; i < m_iNumStates; i++)
			{
				if (strText == m_listStateNames[i])
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
			Debug.Assert(null != m_listMessageNames);

			//loop through messages to find the right one
			for (int i = 0; i < m_iNumMessages; i++)
			{
				if (strText == m_listMessageNames[i])
				{
					//adjust by the message offset
					int iAdjustedMessage = i + m_iMessageOffset;
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
			m_iPrevState = m_iInitialState;
			m_iCurrentState = m_iInitialState;
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
					if ((i < m_iNumStates) && (j < m_iNumMessages) && (m_Data[i, j] < iNumStates))
					{
						pData[i, j] = m_Data[i, j];
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
				if (i < m_iNumStates)
				{
					StateNames[i] = m_listStateNames[i];
				}
			}

			//allocate and copy new message tags
			string[] MessageNames = new string[iNumMessages];
			for (int i = 0; i < iNumMessages; i++)
			{
				if (i < m_iNumMessages)
				{
					MessageNames[i] = m_listMessageNames[i];
				}
			}

			//point to new data
			m_listStateNames = StateNames;
			m_listMessageNames = MessageNames;
			m_Data = pData;
			m_iNumStates = iNumStates;
			m_iNumMessages = iNumMessages;
			ResetToInitialState();
		}

		/// <summary>
		/// Get a state transition
		/// </summary>
		/// <param name="iState">the beginning state</param>
		/// <param name="iMessage">the message to send to that state</param>
		/// <returns>int: the id of the target state when the specified message is sent to the specified state</returns>
		public int GetEntry(int iState, int iMessage)
		{
			Debug.Assert(null != m_Data);

			//adjust the message by the offset
			int iAdjustedMessage = iMessage - m_iMessageOffset;

			Debug.Assert(iState >= 0);
			Debug.Assert(iState < m_iNumStates);
			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < m_iNumMessages);

			return m_Data[iState, iAdjustedMessage];
		}

		/// <summary>
		/// Get the name of a state
		/// </summary>
		/// <param name="iState">teh id of the state to get the name of</param>
		/// <returns>get a state name </returns>
		public string GetStateName(int iState)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < m_iNumStates);
			Debug.Assert(null != m_listStateNames);

			return m_listStateNames[iState];
		}

		/// <summary>
		/// Get the name of a message
		/// </summary>
		/// <param name="iMessage">the id of a message to get the name of</param>
		/// <returns>given a message id, get the message name</returns>
		public string GetMessageName(int iMessage)
		{
			Debug.Assert(null != m_listMessageNames);

			//adjust by the message offset
			int iAdjustedMessage = iMessage - m_iMessageOffset;

			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < m_iNumMessages);

			return m_listMessageNames[iAdjustedMessage];
		}

		/// <summary>
		/// change the name of a state
		/// </summary>
		/// <param name="iState">the state to change</param>
		/// <param name="strStateName">the name to change the state to</param>
		public void SetStateName(int iState, string strStateName)
		{
			Debug.Assert(iState >= 0);
			Debug.Assert(iState < m_iNumStates);
			Debug.Assert(null != m_listStateNames);

			m_listStateNames[iState] = strStateName;
		}

		/// <summary>
		/// change the name of a message
		/// </summary>
		/// <param name="iMessage">id of the message to change the name of</param>
		/// <param name="strMessageName">the name to change the message to</param>
		public void SetMessageName(int iMessage, string strMessageName)
		{
			Debug.Assert(null != m_listMessageNames);

			//adjust by the message offset
			int iAdjustedMessage = iMessage - m_iMessageOffset;

			Debug.Assert(iAdjustedMessage >= 0);
			Debug.Assert(iAdjustedMessage < m_iNumMessages);

			m_listMessageNames[iAdjustedMessage] = strMessageName;
		}

		/// <summary>
		/// Remove a state from the state machine
		/// </summary>
		/// <param name="iState">index of the state to be removed</param>
		public void RemoveState(int iState)
		{
			Debug.Assert(0 <= iState);
			Debug.Assert(iState < m_iNumStates);
			Debug.Assert(m_iNumStates > 0);
			Debug.Assert(null != m_Data);
			Debug.Assert(null != m_listStateNames);

			if (m_iNumStates == 1)
			{
				//can't remove the last state from the state machine
				return;
			}

			//set up a temp array for state data
			int[,] pData = new int[(m_iNumStates - 1), m_iNumMessages];

			//set up temp array for state tags
			string[] pTempStrings = new string[(m_iNumStates - 1)];

			//copy all the data into the new array, except for the state to delete
			int iCurState = 0;
			for (int i = 0; i < m_iNumStates; i++)
			{
				if (iState != iCurState)
				{
					//copy the messages
					for (int j = 0; j < m_iNumMessages; j++)
					{
						//if the state transition goes to the target state, reset it
						if (iState == m_Data[i, j])
						{
							pData[iCurState, j] = iCurState;
						}
						else
						{
							pData[iCurState, j] = m_Data[i, j];
						}
					}

					//copy teh state name
					pTempStrings[iCurState] = m_listStateNames[i];

					//copy the state name
					iCurState++;
				}
			}

			//point to the new data
			m_Data = pData;
			m_listStateNames = pTempStrings;

			//decrement the number of states
			m_iNumStates--;

			//check if the initial state needs to change
			if ((m_iInitialState >= m_iNumStates) || (m_iInitialState == iState))
			{
				m_iInitialState = 0;
			}
		}

		/// <summary>
		/// Remove a message from the state machine
		/// </summary>
		/// <param name="iMessage">index of the message to be removed</param>
		public void RemoveMessage(int iMessage)
		{
			Debug.Assert(null != m_Data);
			Debug.Assert(null != m_listMessageNames);
			Debug.Assert(0 <= iMessage);
			Debug.Assert(iMessage < m_iNumMessages);
			Debug.Assert(m_iNumMessages > 0);

			if (m_iNumMessages == 1)
			{
				//cant remove last message
				return;
			}

			//set up a temp array for state data
			int[,] pData = new int[m_iNumStates, (m_iNumMessages - 1)];

			//set up temp array for message names
			string[] pTempStrings = new string[(m_iNumMessages - 1)];

			//copy all the data into the new array, except for the message to delete
			for (int i = 0; i < m_iNumStates; i++)
			{
				int iCurMessage = 0;
				for (int j = 0; j < m_iNumMessages; j++)
				{
					if (iMessage != j)
					{
						pData[i, iCurMessage] = m_Data[i, j];
						pTempStrings[iCurMessage] = m_listMessageNames[j];
						iCurMessage++;
					}
				}
			}

			//point to the new data
			m_Data = pData;
			m_listMessageNames = pTempStrings;

			//decrement the number of states
			m_iNumMessages--;
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
			if ((m_iInitialState != rInst.m_iInitialState) ||
				(m_iNumStates != rInst.m_iNumStates) ||
				(m_iNumMessages != rInst.m_iNumMessages))
			{
				return false;
			}

			for (int i = 0; i < m_iNumStates; i++)
			{
				if (m_listStateNames[i] != rInst.m_listStateNames[i])
				{
					return false;
				}
			}

			for (int i = 0; i < m_iNumMessages; i++)
			{
				if (m_listMessageNames[i] != rInst.m_listMessageNames[i])
				{
					return false;
				}
			}

			for (int i = 0; i < m_iNumStates; i++)
			{
				for (int j = 0; j < m_iNumMessages; j++)
				{
					if (m_Data[i, j] != rInst.m_Data[i, j])
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion //Methods

		#region Networking

#if !WINDOWS_PHONE

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public virtual void ReadFromNetwork(PacketReader packetReader, IStateContainer rStateContainer)
		{
			//Read the current state and force a state change event if required
			if (ForceState(packetReader.ReadInt32()))
			{
				rStateContainer.StateChange(CurrentState);
			}
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public virtual void WriteToNetwork(PacketWriter packetWriter)
		{
			//the only thing that needs to be synced is the current state!!!
			packetWriter.Write(m_iCurrentState);
		}

#endif

		#endregion //Networking

		#region File IO

#if WINDOWS

		/// <summary>
		/// read in serialized xna state machine from XML
		/// </summary>
		/// <param name="strFilename">file to open</param>
		/// <returns>whether or not it was able to open it</returns>
		public bool ReadSerializedFile(string strFilename)
		{
			// Open the file.
			FileStream stream = File.Open(strFilename, FileMode.Open, FileAccess.Read);
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
			if (!StateNamesNode.HasChildNodes)
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
				m_listStateNames[i] = listStateNames[i];
			}
			for (int i = 0; i < listMessageNames.Count; i++)
			{
				m_listMessageNames[i] = listMessageNames[i];
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
				if (!ReadStateTable(StateTableNode))
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
		public bool AppendSerializedFile(string strFilename)
		{
			// Open the file.
			FileStream stream = File.Open(strFilename, FileMode.Open, FileAccess.Read);
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
				if (!ReadStateTable(StateTableNode))
				{
					Debug.Assert(false);
					return false;
				}
			}

			// Close the file.
			stream.Close();
			return true;
		}

		private bool ReadStateTable(XmlNode StateTableNode)
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
		public void WriteXMLFile(string strFilename)
		{
			//open the file, create it if it doesnt exist yet
			XmlTextWriter rXMLFile = new XmlTextWriter(strFilename, null);
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
			rXMLFile.WriteString(GetStateName(m_iInitialState));
			rXMLFile.WriteEndElement();

			//write out the state names
			rXMLFile.WriteStartElement("stateNames");
			for (int i = 0; i < m_iNumStates; i++)
			{
				rXMLFile.WriteStartElement("Item");
				rXMLFile.WriteAttributeString("Type", "string");
				rXMLFile.WriteString(m_listStateNames[i]);
				rXMLFile.WriteEndElement();
			}
			rXMLFile.WriteEndElement();

			//write out the message names
			rXMLFile.WriteStartElement("messageNames");
			for (int i = 0; i < m_iNumMessages; i++)
			{
				rXMLFile.WriteStartElement("Item");
				rXMLFile.WriteAttributeString("Type", "string");
				rXMLFile.WriteString(m_listMessageNames[i]);
				rXMLFile.WriteEndElement();
			}
			rXMLFile.WriteEndElement();

			//write out all the data
			rXMLFile.WriteStartElement("states");
			for (int i = 0; i < m_iNumStates; i++)
			{
				//write out one state table for each state
				rXMLFile.WriteStartElement("Item");
				rXMLFile.WriteAttributeString("Type", "SPFSettings.StateTableXML");

				//write out the name of the state
				rXMLFile.WriteStartElement("name");
				rXMLFile.WriteString(m_listStateNames[i]);
				rXMLFile.WriteEndElement();

				//write out all teh state transitions
				rXMLFile.WriteStartElement("transitions");

				for (int j = 0; j < m_iNumMessages; j++)
				{
					//Comment this if check out if you want to write out allll state transitions
					if (m_Data[i, j] != i)
					{
						rXMLFile.WriteStartElement("Item");
						rXMLFile.WriteAttributeString("Type", "SPFSettings.StateChangeXML");

						rXMLFile.WriteStartElement("message");
						rXMLFile.WriteString(m_listMessageNames[j]);
						rXMLFile.WriteEndElement();

						rXMLFile.WriteStartElement("state");
						rXMLFile.WriteString(GetStateName(m_Data[i, j]));
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

#endif

		/// <summary>
		/// Load this dude from an xml file.  
		/// This removes all the existing data and loads ALL the data into the state machine!
		/// </summary>
		/// <param name="rContent">the content opbject to load from</param>
		/// <param name="strResource">name of the resource to load from</param>
		/// <param name="iMessageOffset">the message offset to use, if this is a chained state machine</param>
		/// <returns>whether or not any errors ocurred</returns>
		public bool ReadSerializedFile(ContentManager rContent, string strResource, int iMessageOffset)
		{
			//read in serialized xna state machine
			StateMachineXML myXML = rContent.Load<StateMachineXML>(strResource);

			//get teh number of states, message, and fake the initial state
			Set(myXML.stateNames.Count, myXML.messageNames.Count, 0, iMessageOffset);

			//read in the state names
			for (int i = 0; i < m_iNumStates; i++)
			{
				m_listStateNames[i] = myXML.stateNames[i];
			}

			//read in the message names
			for (int i = 0; i < m_iNumMessages; i++)
			{
				m_listMessageNames[i] = myXML.messageNames[i];
			}

			//set teh initial state
			m_iInitialState = GetStateIndexFromText(myXML.initial);
			Debug.Assert(m_iInitialState >= 0);
			Debug.Assert(m_iInitialState < m_iNumStates);

			//read in all the data
			if (!ReadStateTable(myXML.states))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Take a state machine file and insert the data into thisd dude
		/// This keeps all the existing data and adds/appends the file data into the state machine
		/// </summary>
		/// <param name="rContent">the content opbject to load from</param>
		/// <param name="strResource">name of the resource to load from</param>
		/// <param name="iMessageOffset">the message offset to use, if this is a chained state machine</param>
		/// <returns>whether or not any errors ocurred</returns>
		public bool AppendSerializedFile(ContentManager rContent, string strResource, int iMessageOffset)
		{
			m_iMessageOffset = iMessageOffset;

			//read in serialized xna state machine
			SPFSettings.StateMachineXML myXML = rContent.Load<SPFSettings.StateMachineXML>(strResource);

			//read in and append all the state & message names
			ReadNames(myXML.stateNames, myXML.messageNames);

			//set teh initial state
			m_iInitialState = GetStateIndexFromText(myXML.initial);
			Debug.Assert(m_iInitialState >= 0);
			Debug.Assert(m_iInitialState < m_iNumStates);

			//read in all the data
			if (!ReadStateTable(myXML.states))
			{
				return false;
			}

			return true;
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
						m_listStateNames[iNextBlankState] = listStateNames[i];
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
						m_listMessageNames[iNextBlankMessage] = listMessageNames[i];
						iNextBlankMessage++;
					}
				}
			}
		}

		/// <summary>
		/// Read a table of state data into this state machine
		/// </summary>
		/// <param name="listStates">a list of all teh state change objects</param>
		/// <returns>whether or not an error occurred reading in the data</returns>
		private bool ReadStateTable(List<SPFSettings.StateTableXML> listStates)
		{
			for (int i = 0; i < listStates.Count; i++)
			{
				//get the state being described
				int iState = GetStateIndexFromText(listStates[i].name);
				Debug.Assert(iState >= 0);
				Debug.Assert(iState < m_iNumStates);

				for (int j = 0; j < listStates[i].transitions.Count; j++)
				{
					//get the message thats described
					string strMessageName = listStates[i].transitions[j].message;
					int iMessage = GetMessageIndexFromText(strMessageName);
					iMessage -= MessageOffset;
					Debug.Assert(iMessage >= 0);
					Debug.Assert(iMessage < m_iNumMessages);

					//get the target state
					string strTargetState = listStates[i].transitions[j].state;
					int iTargetState = GetStateIndexFromText(strTargetState);
					Debug.Assert(iTargetState >= 0);
					Debug.Assert(iTargetState < m_iNumStates);

					//set the state machine data
					m_Data[iState, iMessage] = iTargetState;
				}
			}

			return true;
		}

		#endregion //File IO
	}
}