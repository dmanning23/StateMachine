using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;

namespace StateMachineBuddy.Tests
{
	public enum TestState
	{
		one,
		two,
		three,
		four,
	}

	public enum TestMessage
	{
		one,
		two,
	}

	[TestFixture]
	public class HybridTests
	{
		HybridStateMachine _states;

		StateMachine _stateMachine;
		StateMachineModel _stateModel;

		[SetUp]
		public void Setup()
		{
			_states = new HybridStateMachine();

			_stateMachine = new StateMachine();
			_stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
			_stateMachine.SetEntry((int)TestState.one, (int)TestMessage.one, (int)TestState.two);
			_stateMachine.SetEntry((int)TestState.one, (int)TestMessage.two, (int)TestState.three);
			_stateMachine.SetEntry((int)TestState.two, (int)TestMessage.one, (int)TestState.three);
			_stateMachine.SetEntry((int)TestState.two, (int)TestMessage.two, (int)TestState.four);

			_stateModel = new StateMachineModel();
			_stateModel.StateNames.Add("five");
			_stateModel.StateNames.Add("six");
			_stateModel.StateNames.Add("seven");
			_stateModel.StateNames.Add("eight");

			_stateModel.MessageNames.Add("three");

			var five = new StateTableModel("five");
			five.Transitions.Add(new StateChangeModel("one", "six"));
			five.Transitions.Add(new StateChangeModel("two", "seven"));
			_stateModel.States.Add(five);

			var six = new StateTableModel("six");
			six.Transitions.Add(new StateChangeModel("one", "seven"));
			six.Transitions.Add(new StateChangeModel("two", "eight"));
			_stateModel.States.Add(six);

			var one = new StateTableModel("one");
			one.Transitions.Add(new StateChangeModel("three", "four"));
			_stateModel.States.Add(one);

			var two = new StateTableModel("two");
			two.Transitions.Add(new StateChangeModel("three", "five"));
			_stateModel.States.Add(two);
		}

		[Test]
		public void InitialState_Invalid()
		{
			_states.AddStateMachine(typeof(TestState), typeof(TestMessage), "one");

			_states.SetInitialState("catpants");

			_states.InitialState.ShouldBe("one");
			_states.PrevState.ShouldBe("one");
			_states.CurrentState.ShouldBe("one");
		}

		[Test]
		public void InitialState_Valid()
		{
			_states.AddStateMachine(typeof(TestState), typeof(TestMessage), "one");

			_states.InitialState.ShouldBe("one");
			_states.PrevState.ShouldBe("one");
			_states.CurrentState.ShouldBe("one");
		}

		[Test]
		public void InitialState2()
		{
			_states.SetStateMachine(_stateMachine);

			_states.InitialState.ShouldBe("one");
			_states.PrevState.ShouldBe("one");
			_states.CurrentState.ShouldBe("one");
		}

		[Test]
		public void NumStates()
		{
			_states.SetStateMachine(_stateMachine);
			_states.StateTable.Count.ShouldBe(_stateMachine.NumStates);
		}

		[Test]
		public void StatesMatch()
		{
			_states.SetStateMachine(_stateMachine);

			for (int i = 0; i < _stateMachine.NumStates; i++)
			{
				var stateName = _stateMachine.GetStateName(i);
				_states.StateTable.Keys.Contains(stateName).ShouldBeTrue();
			}
		}

		[Test]
		public void NumStateName()
		{
			_states.SetStateMachine(_stateMachine);
			_states.States.Count.ShouldBe(_stateMachine.NumStates);
		}

		[Test]
		public void StateNamesMatch()
		{
			_states.SetStateMachine(_stateMachine);

			for (int i = 0; i < _stateMachine.NumStates; i++)
			{
				var stateName = _stateMachine.GetStateName(i);
				_states.States.Contains(stateName).ShouldBeTrue();
			}
		}

		[Test]
		public void NumMessageName()
		{
			_states.SetStateMachine(_stateMachine);
			_states.Messages.Count.ShouldBe(_stateMachine.NumMessages);
		}

		[Test]
		public void MessageNamesMatch()
		{
			_states.SetStateMachine(_stateMachine);

			for (int i = 0; i < _stateMachine.NumMessages; i++)
			{
				var messageName = _stateMachine.GetMessageName(i);
				_states.Messages.Contains(messageName).ShouldBeTrue();
			}
		}

		[Test]
		public void ForceState()
		{
			_states.SetStateMachine(_stateMachine);

			_states.ForceState("two");
			_states.CurrentState.ShouldBe("two");
			_states.PrevState.ShouldBe("one");
		}

		[Test]
		public void ForceState_IgnoresCurrentState()
		{
			_states.SetStateMachine(_stateMachine);

			_states.ForceState("two");
			_states.ForceState("two");
			_states.CurrentState.ShouldBe("two");
			_states.PrevState.ShouldBe("one");
		}

		[TestCase("one", "one", "two")]
		[TestCase("one", "two", "three")]
		[TestCase("two", "one", "three")]
		[TestCase("two", "two", "four")]
		public void StateChanges(string currentState, string message, string expectedNextState)
		{
			_states.SetStateMachine(_stateMachine);
			_states.ForceState(currentState);

			_states.SendStateMessage(message);
			_states.PrevState.ShouldBe(currentState);
			_states.CurrentState.ShouldBe(expectedNextState);
		}

		[Test]
		public void AddWithoutException()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);
		}

		[Test]
		public void AddsStateNames_Count()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			_states.States.Count.ShouldBe(_stateMachine.NumStates + _stateModel.StateNames.Count);
		}

		[Test]
		public void AddsMessageNames_Count()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			_states.Messages.Count.ShouldBe(_stateMachine.NumMessages + _stateModel.MessageNames.Count);
		}

		[Test]
		public void AddsStateNames()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			foreach (var statename in _stateModel.StateNames)
			{
				_states.States.ShouldContain(statename);
			}
		}

		[Test]
		public void AddsMessageNames()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			foreach (var messagename in _stateModel.MessageNames)
			{
				_states.Messages.ShouldContain(messagename);
			}
		}

		[Test]
		public void AddsStateNames_StateTable()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			foreach (var statename in _stateModel.StateNames)
			{
				_states.StateTable.ContainsKey(statename).ShouldBeTrue();
			}
		}

		[TestCase("five", "one", "six")]
		[TestCase("five", "two", "seven")]
		[TestCase("six", "one", "seven")]
		[TestCase("six", "two", "eight")]
		public void StateChanges2(string currentState, string message, string expectedNextState)
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);
			_states.ForceState(currentState);

			_states.SendStateMessage(message);
			_states.PrevState.ShouldBe(currentState);
			_states.CurrentState.ShouldBe(expectedNextState);
		}

		[TestCase("one", "three", "four")]
		[TestCase("two", "three", "five")]
		public void StateChanges3(string currentState, string message, string expectedNextState)
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);
			_states.ForceState(currentState);

			_states.SendStateMessage(message);
			_states.PrevState.ShouldBe(currentState);
			_states.CurrentState.ShouldBe(expectedNextState);
		}

		[Test]
		public void DupeMessageName()
		{
			_states.SetStateMachine(_stateMachine);
			_stateModel.MessageNames.Add("one");

			Should.Throw<Exception>(() => {
				_states.AddStateMachine(_stateModel);
			});
		}

		[Test]
		public void DupeStateName()
		{
			_states.SetStateMachine(_stateMachine);
			_stateModel.StateNames.Add("one");

			Should.Throw<Exception>(() => {
				_states.AddStateMachine(_stateModel);
			});
		}

		[Test]
		public void RemoveMessageNames()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			_states.RemoveStateMachine(_stateModel);

			_states.Messages.Count.ShouldBe(_stateMachine.NumMessages);

			for (int i = 0; i < _stateMachine.NumMessages; i++)
			{
				var messageName = _stateMachine.GetMessageName(i);
				_states.Messages.Contains(messageName).ShouldBeTrue();
			}
		}

		[Test]
		public void RemoveStateNames()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);

			_states.RemoveStateMachine(_stateModel);

			_states.States.Count.ShouldBe(_stateMachine.NumStates);
			_states.StateTable.Count.ShouldBe(_stateMachine.NumStates);

			for (int i = 0; i < _stateMachine.NumStates; i++)
			{
				var stateName = _stateMachine.GetStateName(i);
				_states.States.Contains(stateName).ShouldBeTrue();
				_states.StateTable.ContainsKey(stateName).ShouldBeTrue();
			}
		}

		[TestCase("one", "three", "one")]
		[TestCase("two", "three", "two")]
		public void RemoveTransitions(string currentState, string message, string expectedNextState)
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);
			_states.ForceState(currentState);
			_states.RemoveStateMachine(_stateModel);

			_states.SendStateMessage(message);
			_states.CurrentState.ShouldBe(expectedNextState);
		}

		[Test]
		public void Remove_ResetsCurrent()
		{
			_states.SetStateMachine(_stateMachine);
			_states.AddStateMachine(_stateModel);
			_states.ForceState("five");

			_states.RemoveStateMachine(_stateModel);

			_states.CurrentState.ShouldBe("one");
		}
	}
}
