using System;
using System.Collections.Generic;
using System.Text;

namespace StateMachineBuddy.Models
{
	/// <summary>
	/// this is a list of all the state changes for one state
	/// </summary>
	public class StateTableModel
    {
		#region Properties

		/// <summary>
		/// name of this state, must match one of the states in the state machine
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// list of all the state changes for this state
		/// </summary>
		public List<StateChangeModel> Transitions { get; private set; }

		#endregion //Properties

		#region Methods

		#endregion //Methods
	}
}
