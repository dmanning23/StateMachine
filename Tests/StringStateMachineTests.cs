using NUnit.Framework;
using Shouldly;
using StateMachineBuddy;
using System;

namespace Tests
{
    [TestFixture]
    public class StringStateMachineTests
    {
        private StringStateMachine _stateMachine;

        [SetUp]
        public void Setup()
        {
            _stateMachine = new StringStateMachine();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_Default_InitializesEmpty()
        {
            _stateMachine.States.Count.ShouldBe(0);
            _stateMachine.Messages.Count.ShouldBe(0);
            _stateMachine.StateTable.Count.ShouldBe(0);
            _stateMachine.InitialState.ShouldBeNull();
            _stateMachine.CurrentState.ShouldBeNull();
            _stateMachine.PrevState.ShouldBeNull();
        }

        [Test]
        public void Constructor_Copy_CopiesAllData()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C" });
            _stateMachine.AddMessages(new[] { "X", "Y" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");

            var copy = new StringStateMachine(_stateMachine);

            copy.States.Count.ShouldBe(3);
            copy.Messages.Count.ShouldBe(2);
            copy.InitialState.ShouldBe("A");
            copy.CurrentState.ShouldBe("A");
            copy.StateTable["A"].StateChanges["X"].ShouldBe("B");
        }

        [Test]
        public void Constructor_Copy_IsIndependent()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");

            var copy = new StringStateMachine(_stateMachine);
            _stateMachine.SendStateMessage("X");

            _stateMachine.CurrentState.ShouldBe("B");
            copy.CurrentState.ShouldBe("A");
        }

        #endregion

        #region SetInitialState Tests

        [Test]
        public void SetInitialState_ValidState_SetsState()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C" });

            _stateMachine.SetInitialState("B");

            _stateMachine.InitialState.ShouldBe("B");
            _stateMachine.CurrentState.ShouldBe("B");
            _stateMachine.PrevState.ShouldBe("B");
        }

        [Test]
        public void SetInitialState_InvalidState_DoesNothing()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.SetInitialState("A");

            _stateMachine.SetInitialState("Invalid");

            _stateMachine.InitialState.ShouldBe("A");
        }

        #endregion

        #region AddStates Tests

        [Test]
        public void AddStates_FromCollection_AddsStates()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C" });

            _stateMachine.States.Count.ShouldBe(3);
            _stateMachine.States.ShouldContain("A");
            _stateMachine.States.ShouldContain("B");
            _stateMachine.States.ShouldContain("C");
        }

        [Test]
        public void AddStates_FromCollection_CreatesStateTableEntries()
        {
            _stateMachine.AddStates(new[] { "A", "B" });

            _stateMachine.StateTable.ContainsKey("A").ShouldBeTrue();
            _stateMachine.StateTable.ContainsKey("B").ShouldBeTrue();
        }

        [Test]
        public void AddStates_Duplicate_DoesNotAddTwice()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddStates(new[] { "A", "C" });

            _stateMachine.States.Count.ShouldBe(3);
        }

        [Test]
        public void AddStates_FromEnumType_AddsEnumNames()
        {
            _stateMachine.AddStates(typeof(TestState));

            _stateMachine.States.Count.ShouldBe(4);
            _stateMachine.States.ShouldContain("one");
            _stateMachine.States.ShouldContain("two");
            _stateMachine.States.ShouldContain("three");
            _stateMachine.States.ShouldContain("four");
        }

        #endregion

        #region AddMessages Tests

        [Test]
        public void AddMessages_FromCollection_AddsMessages()
        {
            _stateMachine.AddMessages(new[] { "X", "Y", "Z" });

            _stateMachine.Messages.Count.ShouldBe(3);
            _stateMachine.Messages.ShouldContain("X");
            _stateMachine.Messages.ShouldContain("Y");
            _stateMachine.Messages.ShouldContain("Z");
        }

        [Test]
        public void AddMessages_Duplicate_DoesNotAddTwice()
        {
            _stateMachine.AddMessages(new[] { "X", "Y" });
            _stateMachine.AddMessages(new[] { "X", "Z" });

            _stateMachine.Messages.Count.ShouldBe(3);
        }

        [Test]
        public void AddMessages_FromEnumType_AddsEnumNames()
        {
            _stateMachine.AddMessages(typeof(TestMessage));

            _stateMachine.Messages.Count.ShouldBe(2);
            _stateMachine.Messages.ShouldContain("one");
            _stateMachine.Messages.ShouldContain("two");
        }

        #endregion

        #region AddStateMachine (Enum overload) Tests

        [Test]
        public void AddStateMachine_FromEnums_SetsUpCorrectly()
        {
            _stateMachine.AddStateMachine(typeof(TestState), typeof(TestMessage), "one");

            _stateMachine.States.Count.ShouldBe(4);
            _stateMachine.Messages.Count.ShouldBe(2);
            _stateMachine.InitialState.ShouldBe("one");
        }

        #endregion

        #region Set (Transition) Tests

        [Test]
        public void Set_ValidTransition_AddsTransition()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");

            _stateMachine.Set("A", "X", "B");

            _stateMachine.StateTable["A"].StateChanges["X"].ShouldBe("B");
        }

        [Test]
        public void Set_InvalidSourceState_ThrowsException()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });

            Should.Throw<Exception>(() => _stateMachine.Set("Invalid", "X", "B"));
        }

        [Test]
        public void Set_InvalidMessage_ThrowsException()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });

            Should.Throw<Exception>(() => _stateMachine.Set("A", "Invalid", "B"));
        }

        [Test]
        public void Set_InvalidTargetState_ThrowsException()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });

            Should.Throw<Exception>(() => _stateMachine.Set("A", "X", "Invalid"));
        }

        #endregion

        #region SendStateMessage Tests

        [Test]
        public void SendStateMessage_ValidTransition_ChangesState()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");

            var result = _stateMachine.SendStateMessage("X");

            result.ShouldBeTrue();
            _stateMachine.CurrentState.ShouldBe("B");
            _stateMachine.PrevState.ShouldBe("A");
        }

        [Test]
        public void SendStateMessage_NoTransition_ReturnsFalse()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X", "Y" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");

            var result = _stateMachine.SendStateMessage("Y");

            result.ShouldBeFalse();
            _stateMachine.CurrentState.ShouldBe("A");
        }

        [Test]
        public void SendStateMessage_SelfTransition_ReturnsFalse()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "A");

            var result = _stateMachine.SendStateMessage("X");

            result.ShouldBeFalse();
        }

        [Test]
        public void SendStateMessage_FiresStateChangedEvent()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");

            string oldState = null;
            string newState = null;
            _stateMachine.StateChangedEvent += (sender, args) =>
            {
                oldState = args.OldState;
                newState = args.NewState;
            };

            _stateMachine.SendStateMessage("X");

            oldState.ShouldBe("A");
            newState.ShouldBe("B");
        }

        [Test]
        public void SendStateMessage_NoTransition_DoesNotFireEvent()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");

            bool eventFired = false;
            _stateMachine.StateChangedEvent += (sender, args) => eventFired = true;

            _stateMachine.SendStateMessage("X");

            eventFired.ShouldBeFalse();
        }

        #endregion

        #region ForceState Tests

        [Test]
        public void ForceState_ValidState_ChangesState()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C" });
            _stateMachine.SetInitialState("A");

            _stateMachine.ForceState("C");

            _stateMachine.CurrentState.ShouldBe("C");
            _stateMachine.PrevState.ShouldBe("A");
        }

        [Test]
        public void ForceState_InvalidState_DoesNothing()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.SetInitialState("A");

            _stateMachine.ForceState("Invalid");

            _stateMachine.CurrentState.ShouldBe("A");
        }

        [Test]
        public void ForceState_SameState_DoesNothing()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.SetInitialState("A");

            bool eventFired = false;
            _stateMachine.StateChangedEvent += (sender, args) => eventFired = true;

            _stateMachine.ForceState("A");

            eventFired.ShouldBeFalse();
        }

        [Test]
        public void ForceState_FiresStateChangedEvent()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.SetInitialState("A");

            string oldState = null;
            string newState = null;
            _stateMachine.StateChangedEvent += (sender, args) =>
            {
                oldState = args.OldState;
                newState = args.NewState;
            };

            _stateMachine.ForceState("B");

            oldState.ShouldBe("A");
            newState.ShouldBe("B");
        }

        #endregion

        #region ResetToInitialState Tests

        [Test]
        public void ResetToInitialState_ResetsState()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");
            _stateMachine.SendStateMessage("X");

            _stateMachine.ResetToInitialState();

            _stateMachine.CurrentState.ShouldBe("A");
            _stateMachine.PrevState.ShouldBe("A");
        }

        [Test]
        public void ResetToInitialState_FiresResetEvent()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");
            _stateMachine.SendStateMessage("X");

            bool resetFired = false;
            _stateMachine.ResetEvent += (sender, args) => resetFired = true;

            _stateMachine.ResetToInitialState();

            resetFired.ShouldBeTrue();
        }

        [Test]
        public void ResetToInitialState_DoesNotFireStateChangedEvent()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");
            _stateMachine.SendStateMessage("X");

            bool stateChangedFired = false;
            _stateMachine.StateChangedEvent += (sender, args) => stateChangedFired = true;

            _stateMachine.ResetToInitialState();

            stateChangedFired.ShouldBeFalse();
        }

        #endregion

        #region LoadStateMachine Tests

        [Test]
        public void LoadStateMachine_LoadsStatesAndMessages()
        {
            var model = new StateMachineModel();
            model.StateNames.Add("A");
            model.StateNames.Add("B");
            model.MessageNames.Add("X");
            model.Initial = "A";

            _stateMachine.LoadStateMachine(model);

            _stateMachine.States.Count.ShouldBe(2);
            _stateMachine.Messages.Count.ShouldBe(1);
            _stateMachine.InitialState.ShouldBe("A");
        }

        [Test]
        public void LoadStateMachine_LoadsTransitions()
        {
            var model = new StateMachineModel();
            model.StateNames.Add("A");
            model.StateNames.Add("B");
            model.MessageNames.Add("X");
            model.Initial = "A";

            var stateA = new StateModel { Name = "A" };
            stateA.Transitions.Add(new StateChangeModel("X", "B"));
            model.States.Add(stateA);

            _stateMachine.LoadStateMachine(model);

            _stateMachine.StateTable["A"].StateChanges["X"].ShouldBe("B");
        }

        #endregion

        #region AddStateMachine (Model overload) Tests

        [Test]
        public void AddStateMachine_FromModel_AddsStatesAndMessages()
        {
            // First set up base state machine
            _stateMachine.AddStates(new[] { "Base" });
            _stateMachine.AddMessages(new[] { "BaseMsg" });
            _stateMachine.SetInitialState("Base");

            var model = new StateMachineModel();
            model.StateNames.Add("Added1");
            model.StateNames.Add("Added2");
            model.MessageNames.Add("AddedMsg");

            var state = new StateModel { Name = "Added1" };
            state.Transitions.Add(new StateChangeModel("AddedMsg", "Added2"));
            model.States.Add(state);

            _stateMachine.AddStateMachine(model);

            _stateMachine.States.ShouldContain("Added1");
            _stateMachine.States.ShouldContain("Added2");
            _stateMachine.Messages.ShouldContain("AddedMsg");
        }

        [Test]
        public void AddStateMachine_FromModel_AddsTransitions()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");

            var model = new StateMachineModel();
            model.StateNames.Add("A");
            model.StateNames.Add("B");
            model.MessageNames.Add("X");

            var stateA = new StateModel { Name = "A" };
            stateA.Transitions.Add(new StateChangeModel("X", "B"));
            model.States.Add(stateA);

            _stateMachine.AddStateMachine(model);

            _stateMachine.StateTable["A"].StateChanges["X"].ShouldBe("B");
        }

        #endregion

        #region RemoveStateMachine Tests

        [Test]
        public void RemoveStateMachine_RemovesStates()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C" });
            _stateMachine.AddMessages(new[] { "X", "Y" });
            _stateMachine.SetInitialState("A");

            var model = new StateMachineModel();
            model.StateNames.Add("B");
            model.StateNames.Add("C");
            model.MessageNames.Add("Y");

            _stateMachine.RemoveStateMachine(model);

            _stateMachine.States.ShouldContain("A");
            _stateMachine.States.ShouldNotContain("B");
            _stateMachine.States.ShouldNotContain("C");
        }

        [Test]
        public void RemoveStateMachine_RemovesMessages()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X", "Y" });
            _stateMachine.SetInitialState("A");

            var model = new StateMachineModel();
            model.MessageNames.Add("Y");

            _stateMachine.RemoveStateMachine(model);

            _stateMachine.Messages.ShouldContain("X");
            _stateMachine.Messages.ShouldNotContain("Y");
        }

        [Test]
        public void RemoveStateMachine_RemovesStateTableEntries()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");

            var model = new StateMachineModel();
            model.StateNames.Add("B");

            _stateMachine.RemoveStateMachine(model);

            _stateMachine.StateTable.ContainsKey("B").ShouldBeFalse();
        }

        [Test]
        public void RemoveStateMachine_ForcesStateIfCurrentRemoved()
        {
            _stateMachine.AddStates(new[] { "A", "B" });
            _stateMachine.AddMessages(new[] { "X" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "X", "B");
            _stateMachine.SendStateMessage("X");

            var model = new StateMachineModel();
            model.StateNames.Add("B");

            _stateMachine.RemoveStateMachine(model);

            _stateMachine.CurrentState.ShouldBe("A");
        }

        #endregion

        #region Multiple Transitions Tests

        [Test]
        public void MultipleTransitions_WorkCorrectly()
        {
            _stateMachine.AddStates(new[] { "A", "B", "C", "D" });
            _stateMachine.AddMessages(new[] { "Next" });
            _stateMachine.SetInitialState("A");
            _stateMachine.Set("A", "Next", "B");
            _stateMachine.Set("B", "Next", "C");
            _stateMachine.Set("C", "Next", "D");

            _stateMachine.SendStateMessage("Next");
            _stateMachine.CurrentState.ShouldBe("B");

            _stateMachine.SendStateMessage("Next");
            _stateMachine.CurrentState.ShouldBe("C");

            _stateMachine.SendStateMessage("Next");
            _stateMachine.CurrentState.ShouldBe("D");
        }

        #endregion
    }
}
