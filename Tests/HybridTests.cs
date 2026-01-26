using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using StateMachineBuddy;

namespace Tests
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
        StringStateMachine _stateMachine;

#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
        private StateMachineModel _stateMachineModel;
#pragma warning restore NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

        [SetUp]
        public void Setup()
        {
            _stateMachine = new StringStateMachine();

            _stateMachineModel = new StateMachineModel();
            _stateMachineModel.StateNames.Add("one");
            _stateMachineModel.StateNames.Add("two");
            _stateMachineModel.StateNames.Add("three");
            _stateMachineModel.StateNames.Add("four");
            _stateMachineModel.StateNames.Add("five");
            _stateMachineModel.StateNames.Add("six");
            _stateMachineModel.StateNames.Add("seven");
            _stateMachineModel.StateNames.Add("eight");
            _stateMachineModel.Initial = "five";

            _stateMachineModel.MessageNames.Add("one");
            _stateMachineModel.MessageNames.Add("two");
            _stateMachineModel.MessageNames.Add("three");

            var five = new StateModel() { Name = "five" };
            five.Transitions.Add(new StateChangeModel("one", "six"));
            five.Transitions.Add(new StateChangeModel("two", "seven"));
            _stateMachineModel.States.Add(five);

            var six = new StateModel() { Name = "six" };
            six.Transitions.Add(new StateChangeModel("one", "seven"));
            six.Transitions.Add(new StateChangeModel("two", "eight"));
            _stateMachineModel.States.Add(six);

            var one = new StateModel() { Name = "one" };
            one.Transitions.Add(new StateChangeModel("three", "four"));
            _stateMachineModel.States.Add(one);

            var two = new StateModel() { Name = "two" };
            two.Transitions.Add(new StateChangeModel("three", "five"));
            _stateMachineModel.States.Add(two);
        }

        [Test]
        public void InitialState_Valid()
        {
            _stateMachine.AddStateMachine(typeof(TestState), typeof(TestMessage), "one");

            _stateMachine.InitialState.ShouldBe("one");
            _stateMachine.PrevState.ShouldBe("one");
            _stateMachine.CurrentState.ShouldBe("one");
        }

        [Test]
        public void InitialState_Invalid()
        {
            _stateMachine.AddStateMachine(typeof(TestState), typeof(TestMessage), "catpants");

            _stateMachine.InitialState.ShouldBeNull();
            _stateMachine.PrevState.ShouldBeNull();
            _stateMachine.CurrentState.ShouldBeNull();
        }

        [Test]
        public void SetInitialState_Valid()
        {
            _stateMachine.AddStateMachine(typeof(TestState), typeof(TestMessage), "one");
            _stateMachine.SetInitialState("two");

            _stateMachine.InitialState.ShouldBe("two");
            _stateMachine.PrevState.ShouldBe("two");
            _stateMachine.CurrentState.ShouldBe("two");
        }

        [Test]
        public void SetInitialState_Invalid()
        {
            _stateMachine.AddStateMachine(typeof(TestState), typeof(TestMessage), "one");
            _stateMachine.SetInitialState("catpants");

            _stateMachine.InitialState.ShouldBe("one");
            _stateMachine.PrevState.ShouldBe("one");
            _stateMachine.CurrentState.ShouldBe("one");
        }

        [Test]
        public void InitialState()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.InitialState.ShouldBe("five");
        }

        [Test]
        public void NumStateNames()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.States.Count.ShouldBe(_stateMachineModel.StateNames.Count);
        }

        [Test]
        public void NumMessageNames()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.Messages.Count.ShouldBe(_stateMachineModel.MessageNames.Count);
        }

        [Test]
        public void StateNamesMatch()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            foreach (var stateModel in _stateMachineModel.States)
            {
                _stateMachine.States.Contains(stateModel.Name).ShouldBeTrue();
            }
        }

        [Test]
        public void MessageNamesMatch()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            foreach (var messageName in _stateMachineModel.MessageNames)
            {
                _stateMachine.Messages.Contains(messageName).ShouldBeTrue();
            }
        }

        [Test]
        public void StatesMatch()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            foreach (var stateModel in _stateMachineModel.States)
            {
                _stateMachine.StateTable.Keys.Contains(stateModel.Name).ShouldBeTrue();
            }
        }

        [Test]
        public void ForceState()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.ForceState("two");

            _stateMachine.CurrentState.ShouldBe("two");
            _stateMachine.PrevState.ShouldBe("five");
        }

        [Test]
        public void ForceState_IgnoresCurrentState()
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.ForceState("two");
            _stateMachine.ForceState("two");

            _stateMachine.CurrentState.ShouldBe("two");
            _stateMachine.PrevState.ShouldBe("five");
        }

        [TestCase("one", "one", "two")]
        [TestCase("one", "two", "three")]
        [TestCase("two", "one", "three")]
        [TestCase("two", "two", "four")]
        public void InvalideStateChanges(string currentState, string message, string expectedNextState)
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.ForceState(currentState);
            _stateMachine.SendStateMessage(message);

            _stateMachine.PrevState.ShouldBe("five");
            _stateMachine.CurrentState.ShouldBe(currentState);
        }

        [TestCase("five", "one", "six")]
        [TestCase("five", "two", "seven")]
        [TestCase("six", "one", "seven")]
        [TestCase("six", "two", "eight")]
        public void StateChanges2(string currentState, string message, string expectedNextState)
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.ForceState(currentState);
            _stateMachine.SendStateMessage(message);

            _stateMachine.PrevState.ShouldBe(currentState);
            _stateMachine.CurrentState.ShouldBe(expectedNextState);
        }

        [TestCase("one", "three", "four")]
        [TestCase("two", "three", "five")]
        public void StateChanges3(string currentState, string message, string expectedNextState)
        {
            _stateMachine.LoadStateMachine(_stateMachineModel);

            _stateMachine.ForceState(currentState);
            _stateMachine.SendStateMessage(message);

            _stateMachine.PrevState.ShouldBe(currentState);
            _stateMachine.CurrentState.ShouldBe(expectedNextState);
        }
    }
}
