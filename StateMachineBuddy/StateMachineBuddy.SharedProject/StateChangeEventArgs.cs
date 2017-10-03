using System;

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
		public StateChangeEventArgs(int oldState, int newState)
		{
			OldState = oldState;
			NewState = newState;
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
