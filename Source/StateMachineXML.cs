using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StateMachineBuddy
{
	/// <summary>
	/// this object records the change from one state to another when the state machine receives a message
	/// </summary>
	public class StateChangeXML
	{
		/// <summary>
		/// message recieved, must match one of teh messages in the state machine
		/// </summary>
		public string message = "";

		/// <summary>
		/// target state, must match one of the states in the state machine
		/// </summary>
		public string state = "";
	}

	/// <summary>
	/// this is a list of all the state changes for one state
	/// </summary>
	public class StateTableXML
	{
		/// <summary>
		/// name of this state, must match one of the states in the state machine
		/// </summary>
		public string name = "";

		/// <summary>
		/// list of all the state changes for this state
		/// </summary>
		public List<StateChangeXML> transitions = new List<StateChangeXML>();
	}

	/// <summary>
	/// this is all the data for a state machine
	/// </summary>
	public class StateMachineXML
	{
		/// <summary>
		/// the initial state of this state machine
		/// </summary>
		public string initial = "";

		/// <summary>
		/// list of all the states in the state machine
		/// </summary>
		public List<string> stateNames = new List<string>();

		/// <summary>
		/// list of all the messages in the state machine
		/// </summary>
		public List<string> messageNames = new List<string>();

		/// <summary>
		/// all the state transition data for this state machine
		/// </summary>
		public List<StateTableXML> states = new List<StateTableXML>();
	}
}