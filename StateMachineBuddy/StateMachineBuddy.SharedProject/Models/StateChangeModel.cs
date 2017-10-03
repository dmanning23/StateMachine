using System;
using System.Collections.Generic;
using System.Text;

namespace StateMachineBuddy.Models
{
	/// <summary>
	/// this object records the change from one state to another when the state machine receives a message
	/// </summary>
	public class StateChangeModel
    {
		#region Properties

		/// <summary>
		/// message recieved, must match one of teh messages in the state machine
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// target state, must match one of the states in the state machine
		/// </summary>
		public string State { get; private set; }

		#endregion //Properties

		#region Methods

		#endregion //Methods
	}
}
