using System;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace StateMachineBuddy
{
	public interface IStateMachine<T>
	{
		T CurrentState { get; }
		string CurrentStateText { get; }
		T InitialState { get; set; }
		string[] MessageNames { get; }
		int MessageOffset { get; set; }
		int NumMessages { get; }
		int NumStates { get; }
		T PrevState { get; }
		string[] StateNames { get; }

		event EventHandler<StateChangeEventArgs> ResetEvent;
		event EventHandler<StateChangeEventArgs> StateChangedEvent;

		void AppendXml(Filename file, ContentManager xmlContent);
		bool Compare(StateMachine inst);
		void ForceState(int state);
		T GetEntry(T state, T message);
		T GetMessageFromName(string messageName);
		string GetMessageName(T message);
		T GetStateFromName(string stateName);
		string GetStateName(T state);
		void LoadXml(Filename file, ContentManager xmlContent);
		void RemoveMessage(T message);
		void RemoveState(T state);
		void ResetToInitialState();
		void Resize(int numStates, int numMessages);
		bool SendStateMessage(T message);
		void Set(int numStates, int numMessages, T initialState, int offset = 0);
		void Set(Type statesEnum, Type messagesEnum, T initialState, int offset = 0);
		void SetEntry(T state, T message, T nextState);
		void SetEntry(T state, string messageName, string nextStateName);
		void SetMessageName(T message, string messageName);
		void SetNames(Type states, bool isStates);
		void SetStateName(T state, string stateName);
		void WriteXml(Filename file);
	}
}