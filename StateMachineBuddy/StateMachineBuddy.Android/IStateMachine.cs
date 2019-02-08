using System;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace StateMachineBuddy
{
	public interface IStateMachine
	{
		int CurrentState { get; }
		string CurrentStateText { get; }
		int InitialState { get; set; }
		string[] MessageNames { get; }
		int MessageOffset { get; set; }
		int NumMessages { get; }
		int NumStates { get; }
		int PrevState { get; }
		string[] StateNames { get; }

		event EventHandler<StateChangeEventArgs> ResetEvent;
		event EventHandler<StateChangeEventArgs> StateChangedEvent;

		void AppendXml(Filename file, ContentManager xmlContent);
		bool Compare(StateMachine inst);
		void ForceState(int state);
		int GetEntry(int state, int message);
		int GetMessageFromName(string messageName);
		string GetMessageName(int message);
		int GetStateFromName(string stateName);
		string GetStateName(int state);
		void LoadXml(Filename file, ContentManager xmlContent);
		void RemoveMessage(int message);
		void RemoveState(int state);
		void ResetToInitialState();
		void Resize(int numStates, int numMessages);
		void SendStateMessage(int message);
		void Set(int numStates, int numMessages, int initialState = 0, int offset = 0);
		void Set(Type statesEnum, Type messagesEnum, int initialState = 0, int offset = 0);
		void SetEntry(int state, int message, int nextState);
		void SetEntry(int state, string messageName, string nextStateName);
		void SetMessageName(int message, string messageName);
		void SetNames(Type states, bool isStates);
		void SetStateName(int state, string stateName);
		void WriteXml(Filename file);
	}
}