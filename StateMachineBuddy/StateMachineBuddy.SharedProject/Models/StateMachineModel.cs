using System;
using System.Collections.Generic;
using System.Text;
using XmlBuddy;
using FilenameBuddy;

namespace StateMachineBuddy.Models
{
	/// <summary>
	/// this is all the data for a state machine
	/// </summary>
	public class StateMachineModel : XmlFileBuddy
    {
		#region Properties

		/// <summary>
		/// the initial state of this state machine
		/// </summary>
		public string Initial { get; private set; }

		/// <summary>
		/// list of all the states in the state machine
		/// </summary>
		public List<string> StateNames { get; private set; }

		/// <summary>
		/// list of all the messages in the state machine
		/// </summary>
		public List<string> MessageNames { get; private set; }

		/// <summary>
		/// all the state transition data for this state machine
		/// </summary>
		public List<StateTableModel> States { get; private set; }

		#endregion //Properties

		#region Methods

		#endregion //Methods
	}
}
