using NUnit.Framework;
using Shouldly;
using StateMachineBuddy;
using System;

namespace Tests
{
    [TestFixture]
    public class StateTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Default_InitializesEmptyDictionary()
        {
            var state = new State();

            state.StateChanges.ShouldNotBeNull();
            state.StateChanges.Count.ShouldBe(0);
        }

        [Test]
        public void Constructor_Copy_CopiesAllTransitions()
        {
            var original = new State();
            original.StateChanges["A"] = "B";
            original.StateChanges["X"] = "Y";

            var copy = new State(original);

            copy.StateChanges.Count.ShouldBe(2);
            copy.StateChanges["A"].ShouldBe("B");
            copy.StateChanges["X"].ShouldBe("Y");
        }

        [Test]
        public void Constructor_Copy_IsIndependent()
        {
            var original = new State();
            original.StateChanges["A"] = "B";

            var copy = new State(original);
            original.StateChanges["A"] = "C";

            copy.StateChanges["A"].ShouldBe("B");
        }

        #endregion

        #region StateChanges Tests

        [Test]
        public void StateChanges_CanAddTransitions()
        {
            var state = new State();

            state.StateChanges["Message1"] = "TargetState1";
            state.StateChanges["Message2"] = "TargetState2";

            state.StateChanges.Count.ShouldBe(2);
        }

        [Test]
        public void StateChanges_CanUpdateTransitions()
        {
            var state = new State();
            state.StateChanges["Message"] = "StateA";

            state.StateChanges["Message"] = "StateB";

            state.StateChanges["Message"].ShouldBe("StateB");
        }

        [Test]
        public void StateChanges_CanRemoveTransitions()
        {
            var state = new State();
            state.StateChanges["Message"] = "State";

            state.StateChanges.Remove("Message");

            state.StateChanges.ContainsKey("Message").ShouldBeFalse();
        }

        #endregion

        #region AddStateMachine Tests

        [Test]
        public void AddStateMachine_AddsTransitions()
        {
            var state = new State();
            var stateMachine = new StringStateMachine();
            stateMachine.AddStates(new[] { "A", "B", "C" });
            stateMachine.AddMessages(new[] { "X", "Y" });
            stateMachine.SetInitialState("A");

            var stateModel = new StateModel { Name = "A" };
            stateModel.Transitions.Add(new StateChangeModel("X", "B"));
            stateModel.Transitions.Add(new StateChangeModel("Y", "C"));

            state.AddStateMachine(stateModel, stateMachine, "A");

            state.StateChanges["X"].ShouldBe("B");
            state.StateChanges["Y"].ShouldBe("C");
        }

        [Test]
        public void AddStateMachine_IgnoresSelfTransitions()
        {
            var state = new State();
            var stateMachine = new StringStateMachine();
            stateMachine.AddStates(new[] { "A", "B" });
            stateMachine.AddMessages(new[] { "X" });
            stateMachine.SetInitialState("A");

            var stateModel = new StateModel { Name = "A" };
            stateModel.Transitions.Add(new StateChangeModel("X", "A"));

            state.AddStateMachine(stateModel, stateMachine, "A");

            state.StateChanges.ContainsKey("X").ShouldBeFalse();
        }

        [Test]
        public void AddStateMachine_ThrowsForInvalidMessage()
        {
            var state = new State();
            var stateMachine = new StringStateMachine();
            stateMachine.AddStates(new[] { "A", "B" });
            stateMachine.AddMessages(new[] { "X" });
            stateMachine.SetInitialState("A");

            var stateModel = new StateModel { Name = "A" };
            stateModel.Transitions.Add(new StateChangeModel("Invalid", "B"));

            Should.Throw<Exception>(() => state.AddStateMachine(stateModel, stateMachine, "A"));
        }

        [Test]
        public void AddStateMachine_ThrowsForInvalidTargetState()
        {
            var state = new State();
            var stateMachine = new StringStateMachine();
            stateMachine.AddStates(new[] { "A", "B" });
            stateMachine.AddMessages(new[] { "X" });
            stateMachine.SetInitialState("A");

            var stateModel = new StateModel { Name = "A" };
            stateModel.Transitions.Add(new StateChangeModel("X", "Invalid"));

            Should.Throw<Exception>(() => state.AddStateMachine(stateModel, stateMachine, "A"));
        }

        #endregion

        #region RemoveStateMachine Tests

        [Test]
        public void RemoveStateMachine_RemovesMessages()
        {
            var state = new State();
            state.StateChanges["X"] = "B";
            state.StateChanges["Y"] = "C";
            state.StateChanges["Z"] = "D";

            var model = new StateMachineModel();
            model.MessageNames.Add("Y");

            state.RemoveStateMachine(model);

            state.StateChanges.ContainsKey("X").ShouldBeTrue();
            state.StateChanges.ContainsKey("Y").ShouldBeFalse();
            state.StateChanges.ContainsKey("Z").ShouldBeTrue();
        }

        [Test]
        public void RemoveStateMachine_RemovesTransitionsToRemovedStates()
        {
            var state = new State();
            state.StateChanges["X"] = "B";
            state.StateChanges["Y"] = "C";
            state.StateChanges["Z"] = "B";

            var model = new StateMachineModel();
            model.StateNames.Add("B");

            state.RemoveStateMachine(model);

            state.StateChanges.ContainsKey("X").ShouldBeFalse();
            state.StateChanges.ContainsKey("Y").ShouldBeTrue();
            state.StateChanges.ContainsKey("Z").ShouldBeFalse();
        }

        [Test]
        public void RemoveStateMachine_RemovesBothMessagesAndTargetStates()
        {
            var state = new State();
            state.StateChanges["X"] = "B";
            state.StateChanges["Y"] = "C";
            state.StateChanges["Z"] = "D";

            var model = new StateMachineModel();
            model.MessageNames.Add("X");
            model.StateNames.Add("D");

            state.RemoveStateMachine(model);

            state.StateChanges.ContainsKey("X").ShouldBeFalse();
            state.StateChanges.ContainsKey("Y").ShouldBeTrue();
            state.StateChanges.ContainsKey("Z").ShouldBeFalse();
        }

        #endregion
    }
}
