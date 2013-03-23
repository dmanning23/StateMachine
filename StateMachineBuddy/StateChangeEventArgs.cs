using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateMachineBuddy
{
	/// <summary>
	/// custom event argument that includes the previous and new state
	/// </summary>
	public class StateChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public StateChangeEventArgs(int iOldState, int iNewState)
		{
			OldState = iOldState;
			NewState = iNewState;
		}

		/// <summary>
		/// The previous state
		/// </summary>
		public int OldState { get; set; }

		/// <summary>
		/// the new state we just changed to
		/// </summary>
		public int NewState { get; set; }
	}
}
