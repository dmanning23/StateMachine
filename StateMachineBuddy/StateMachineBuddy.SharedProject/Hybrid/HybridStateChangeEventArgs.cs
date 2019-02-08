using System;

namespace StateMachineBuddy
{
	/// <summary>
	/// custom event argument that includes the previous and new state
	/// </summary>
	public class HybridStateChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public HybridStateChangeEventArgs(string oldState, string newState)
		{
			OldState = oldState;
			NewState = newState;
		}

		/// <summary>
		/// The previous state
		/// </summary>
		public string OldState { get; set; }

		/// <summary>
		/// the new state we just changed to
		/// </summary>
		public string NewState { get; set; }
	}
}
