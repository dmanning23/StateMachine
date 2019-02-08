using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMachineBuddy
{
	public class State
	{
		#region Properties

		public Dictionary<string, string> StateChanges { get; set; }

		#endregion //Properties

		#region Methods

		public State()
		{
			StateChanges = new Dictionary<string, string>();
		}

		public void AddStateMachine(StateMachine stateMachine, string stateName)
		{
			//get the index of this state
			var stateIndex = stateMachine.GetStateFromName(stateName);

			//loop through all the messages and add state changes as necessary
			for (var i = 0; i < stateMachine.NumMessages; i++)
			{
				var messageName = stateMachine.GetMessageName(i);
				var targetState = stateMachine.GetEntry(stateIndex, i);
				if (stateIndex != targetState)
				{
					//add a target state for this change
					var targetStateName = stateMachine.GetStateName(targetState);
					StateChanges[messageName] = targetStateName;
				}
			}
		}

		public void AddStateMachine(StateTableModel stateTable, HybridStateMachine stateMachine, string stateName)
		{
			foreach (var change in stateTable.Transitions)
			{
				if (!stateMachine.Messages.Contains(change.Message))
				{
					throw new Exception($"State machine doesn't have message {change.Message}");
				}
				if (!stateMachine.States.Contains(change.State))
				{
					throw new Exception($"State machine doesn't have state {change.State}");
				}

				if (stateName != change.State)
				{
					StateChanges[change.Message] = change.State;
				}
			}
		}

		public void RemoveStateMachine(StateMachineModel stateModel)
		{
			//remove the messages
			foreach (var message in stateModel.MessageNames)
			{
				StateChanges.Remove(message);
			}

			//remove the targetstates
			foreach (var state in stateModel.StateNames)
			{
				foreach (var targetState in StateChanges.Where(x => x.Value == state).ToList())
				{
					StateChanges.Remove(targetState.Key);
				}
			}
		}

		#endregion //Methods
	}
}
