using NUnit.Framework;
using Shouldly;
using StateMachineBuddy;
using System;

namespace Tests
{
    [TestFixture]
    public class IntStateMachineTests
    {
        private IntStateMachine _stateMachine;

        [SetUp]
        public void Setup()
        {
            _stateMachine = new IntStateMachine();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_InitializesWithDefaultValues()
        {
            _stateMachine.InitialState.ShouldBe(0);
            _stateMachine.CurrentState.ShouldBe(0);
            _stateMachine.PrevState.ShouldBe(0);
            _stateMachine.NumStates.ShouldBe(0);
            _stateMachine.NumMessages.ShouldBe(0);
        }

        #endregion

        #region Set Tests

        [Test]
        public void Set_WithCounts_InitializesCorrectly()
        {
            _stateMachine.Set(3, 2, 1);

            _stateMachine.NumStates.ShouldBe(3);
            _stateMachine.NumMessages.ShouldBe(2);
            _stateMachine.InitialState.ShouldBe(1);
            _stateMachine.CurrentState.ShouldBe(1);
            _stateMachine.PrevState.ShouldBe(1);
        }

        [Test]
        public void Set_WithCounts_DefaultTransitionsToSelf()
        {
            _stateMachine.Set(3, 2);

            for (int state = 0; state < 3; state++)
            {
                for (int message = 0; message < 2; message++)
                {
                    _stateMachine.GetEntry(state, message).ShouldBe(state);
                }
            }
        }

        [Test]
        public void Set_WithEnumTypes_InitializesFromEnums()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.NumStates.ShouldBe(4);
            _stateMachine.NumMessages.ShouldBe(2);
            _stateMachine.GetStateName(0).ShouldBe("one");
            _stateMachine.GetMessageName(0).ShouldBe("one");
        }

        #endregion

        #region SetEntry Tests

        [Test]
        public void SetEntry_WithInts_SetsTransition()
        {
            _stateMachine.Set(3, 2);

            _stateMachine.SetEntry(0, 0, 1);

            _stateMachine.GetEntry(0, 0).ShouldBe(1);
        }

        [Test]
        public void SetEntry_WithNames_SetsTransition()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.SetEntry(0, "one", "two");

            _stateMachine.GetEntry(0, 0).ShouldBe(1);
        }

        #endregion

        #region SetNames Tests

        [Test]
        public void SetNames_ForStates_SetsStateNames()
        {
            _stateMachine.Set(4, 2);

            _stateMachine.SetNames(typeof(TestState), true);

            _stateMachine.GetStateName(0).ShouldBe("one");
            _stateMachine.GetStateName(1).ShouldBe("two");
            _stateMachine.GetStateName(2).ShouldBe("three");
            _stateMachine.GetStateName(3).ShouldBe("four");
        }

        [Test]
        public void SetNames_ForMessages_SetsMessageNames()
        {
            _stateMachine.Set(4, 2);

            _stateMachine.SetNames(typeof(TestMessage), false);

            _stateMachine.GetMessageName(0).ShouldBe("one");
            _stateMachine.GetMessageName(1).ShouldBe("two");
        }

        #endregion

        #region SetStateName / SetMessageName Tests

        [Test]
        public void SetStateName_SetsName()
        {
            _stateMachine.Set(3, 2);

            _stateMachine.SetStateName(0, "Alpha");
            _stateMachine.SetStateName(1, "Beta");

            _stateMachine.GetStateName(0).ShouldBe("Alpha");
            _stateMachine.GetStateName(1).ShouldBe("Beta");
        }

        [Test]
        public void SetMessageName_SetsName()
        {
            _stateMachine.Set(3, 2);

            _stateMachine.SetMessageName(0, "Start");
            _stateMachine.SetMessageName(1, "Stop");

            _stateMachine.GetMessageName(0).ShouldBe("Start");
            _stateMachine.GetMessageName(1).ShouldBe("Stop");
        }

        #endregion

        #region SendStateMessage Tests

        [Test]
        public void SendStateMessage_WithValidTransition_ChangesState()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 1);

            var result = _stateMachine.SendStateMessage(0);

            result.ShouldBeTrue();
            _stateMachine.CurrentState.ShouldBe(1);
            _stateMachine.PrevState.ShouldBe(0);
        }

        [Test]
        public void SendStateMessage_WithNoTransition_ReturnsFalse()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            var result = _stateMachine.SendStateMessage(0);

            result.ShouldBeFalse();
            _stateMachine.CurrentState.ShouldBe(0);
        }

        [Test]
        public void SendStateMessage_WithSelfTransition_ReturnsFalse()
        {
            _stateMachine.Set(3, 2);
            _stateMachine.SetEntry(0, 0, 0);

            var result = _stateMachine.SendStateMessage(0);

            result.ShouldBeFalse();
        }

        [Test]
        public void SendStateMessage_FiresStateChangedEvent()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 1);

            int oldState = -1;
            int newState = -1;
            _stateMachine.StateChangedEvent += (sender, args) =>
            {
                oldState = args.OldState;
                newState = args.NewState;
            };

            _stateMachine.SendStateMessage(0);

            oldState.ShouldBe(0);
            newState.ShouldBe(1);
        }

        #endregion

        #region ForceState Tests

        [Test]
        public void ForceState_ChangesState()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.ForceState(2);

            _stateMachine.CurrentState.ShouldBe(2);
            _stateMachine.PrevState.ShouldBe(0);
        }

        [Test]
        public void ForceState_ToSameState_DoesNotFireEvent()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            bool eventFired = false;
            _stateMachine.StateChangedEvent += (sender, args) => eventFired = true;

            _stateMachine.ForceState(0);

            eventFired.ShouldBeFalse();
        }

        [Test]
        public void ForceState_FiresStateChangedEvent()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            int oldState = -1;
            int newState = -1;
            _stateMachine.StateChangedEvent += (sender, args) =>
            {
                oldState = args.OldState;
                newState = args.NewState;
            };

            _stateMachine.ForceState(2);

            oldState.ShouldBe(0);
            newState.ShouldBe(2);
        }

        #endregion

        #region ResetToInitialState Tests

        [Test]
        public void ResetToInitialState_ResetsState()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 2);
            _stateMachine.SendStateMessage(0);

            _stateMachine.ResetToInitialState();

            _stateMachine.CurrentState.ShouldBe(0);
            _stateMachine.PrevState.ShouldBe(0);
        }

        [Test]
        public void ResetToInitialState_FiresResetEvent()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 2);
            _stateMachine.SendStateMessage(0);

            bool resetFired = false;
            _stateMachine.ResetEvent += (sender, args) => resetFired = true;

            _stateMachine.ResetToInitialState();

            resetFired.ShouldBeTrue();
        }

        [Test]
        public void ResetToInitialState_DoesNotFireStateChangedEvent()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 2);
            _stateMachine.SendStateMessage(0);

            bool stateChangedFired = false;
            _stateMachine.StateChangedEvent += (sender, args) => stateChangedFired = true;

            _stateMachine.ResetToInitialState();

            stateChangedFired.ShouldBeFalse();
        }

        #endregion

        #region GetStateFromName / GetMessageFromName Tests

        [Test]
        public void GetStateFromName_ReturnsCorrectIndex()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.GetStateFromName("one").ShouldBe(0);
            _stateMachine.GetStateFromName("two").ShouldBe(1);
            _stateMachine.GetStateFromName("three").ShouldBe(2);
        }

        [Test]
        public void GetStateFromName_ReturnsNegativeOneForNotFound()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.GetStateFromName("nonexistent").ShouldBe(-1);
        }

        [Test]
        public void GetMessageFromName_ReturnsCorrectIndex()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.GetMessageFromName("one").ShouldBe(0);
            _stateMachine.GetMessageFromName("two").ShouldBe(1);
        }

        [Test]
        public void GetMessageFromName_ReturnsNegativeOneForNotFound()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.GetMessageFromName("nonexistent").ShouldBe(-1);
        }

        #endregion

        #region InitialState Property Tests

        [Test]
        public void InitialState_SetToValidValue_SetsValue()
        {
            _stateMachine.Set(3, 2, 0);

            _stateMachine.InitialState = 2;

            _stateMachine.InitialState.ShouldBe(2);
        }

        [Test]
        public void InitialState_SetToInvalidValue_SetsToZero()
        {
            _stateMachine.Set(3, 2, 1);

            _stateMachine.InitialState = 10;

            _stateMachine.InitialState.ShouldBe(0);
        }

        [Test]
        public void InitialState_SetToNegative_SetsToZero()
        {
            _stateMachine.Set(3, 2, 1);

            _stateMachine.InitialState = -1;

            _stateMachine.InitialState.ShouldBe(0);
        }

        #endregion

        #region CurrentStateName / InitialStateName Tests

        [Test]
        public void CurrentStateName_ReturnsCorrectName()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.CurrentStateName.ShouldBe("one");
        }

        [Test]
        public void InitialStateName_ReturnsCorrectName()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 1);

            _stateMachine.InitialStateName.ShouldBe("two");
        }

        #endregion

        #region Resize Tests

        [Test]
        public void Resize_IncreasesSize()
        {
            _stateMachine.Set(2, 2, 0);
            _stateMachine.SetStateName(0, "A");
            _stateMachine.SetStateName(1, "B");

            _stateMachine.Resize(4, 3);

            _stateMachine.NumStates.ShouldBe(4);
            _stateMachine.NumMessages.ShouldBe(3);
            _stateMachine.GetStateName(0).ShouldBe("A");
            _stateMachine.GetStateName(1).ShouldBe("B");
        }

        [Test]
        public void Resize_DecreasesSize()
        {
            _stateMachine.Set(4, 3, 0);

            _stateMachine.Resize(2, 2);

            _stateMachine.NumStates.ShouldBe(2);
            _stateMachine.NumMessages.ShouldBe(2);
        }

        [Test]
        public void Resize_PreservesTransitions()
        {
            _stateMachine.Set(3, 2, 0);
            _stateMachine.SetEntry(0, 0, 1);

            _stateMachine.Resize(4, 3);

            _stateMachine.GetEntry(0, 0).ShouldBe(1);
        }

        [Test]
        public void Resize_WithZeroStates_DoesNothing()
        {
            _stateMachine.Set(3, 2, 0);

            _stateMachine.Resize(0, 2);

            _stateMachine.NumStates.ShouldBe(3);
        }

        [Test]
        public void Resize_WithZeroMessages_DoesNothing()
        {
            _stateMachine.Set(3, 2, 0);

            _stateMachine.Resize(3, 0);

            _stateMachine.NumMessages.ShouldBe(2);
        }

        #endregion

        #region RemoveState Tests

        [Test]
        public void RemoveState_RemovesState()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.RemoveState(1);

            _stateMachine.NumStates.ShouldBe(3);
        }

        [Test]
        public void RemoveState_LastState_DoesNotRemove()
        {
            _stateMachine.Set(1, 2, 0);

            _stateMachine.RemoveState(0);

            _stateMachine.NumStates.ShouldBe(1);
        }

        [Test]
        public void RemoveState_ResetsInitialStateIfNeeded()
        {
            _stateMachine.Set(3, 2, 2);

            _stateMachine.RemoveState(2);

            _stateMachine.InitialState.ShouldBe(0);
        }

        #endregion

        #region RemoveMessage Tests

        [Test]
        public void RemoveMessage_RemovesMessage()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            _stateMachine.RemoveMessage(1);

            _stateMachine.NumMessages.ShouldBe(1);
        }

        [Test]
        public void RemoveMessage_LastMessage_DoesNotRemove()
        {
            _stateMachine.Set(2, 1, 0);

            _stateMachine.RemoveMessage(0);

            _stateMachine.NumMessages.ShouldBe(1);
        }

        #endregion

        #region Compare Tests

        [Test]
        public void Compare_IdenticalMachines_ReturnsTrue()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 1);

            var other = new IntStateMachine();
            other.Set(typeof(TestState), typeof(TestMessage), 0);
            other.SetEntry(0, 0, 1);

            _stateMachine.Compare(other).ShouldBeTrue();
        }

        [Test]
        public void Compare_DifferentInitialState_ReturnsFalse()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);

            var other = new IntStateMachine();
            other.Set(typeof(TestState), typeof(TestMessage), 1);

            _stateMachine.Compare(other).ShouldBeFalse();
        }

        [Test]
        public void Compare_DifferentNumStates_ReturnsFalse()
        {
            _stateMachine.Set(3, 2, 0);

            var other = new IntStateMachine();
            other.Set(4, 2, 0);

            _stateMachine.Compare(other).ShouldBeFalse();
        }

        [Test]
        public void Compare_DifferentTransitions_ReturnsFalse()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 1);

            var other = new IntStateMachine();
            other.Set(typeof(TestState), typeof(TestMessage), 0);
            other.SetEntry(0, 0, 2);

            _stateMachine.Compare(other).ShouldBeFalse();
        }

        [Test]
        public void Compare_DifferentStateNames_ReturnsFalse()
        {
            _stateMachine.Set(3, 2, 0);
            _stateMachine.SetStateName(0, "A");

            var other = new IntStateMachine();
            other.Set(3, 2, 0);
            other.SetStateName(0, "B");

            _stateMachine.Compare(other).ShouldBeFalse();
        }

        #endregion

        #region GetEntry Tests

        [Test]
        public void GetEntry_ReturnsCorrectTarget()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 2);
            _stateMachine.SetEntry(1, 1, 3);

            _stateMachine.GetEntry(0, 0).ShouldBe(2);
            _stateMachine.GetEntry(1, 1).ShouldBe(3);
        }

        #endregion

        #region Multiple Transitions Tests

        [Test]
        public void MultipleTransitions_WorkCorrectly()
        {
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry(0, 0, 1);
            _stateMachine.SetEntry(1, 0, 2);
            _stateMachine.SetEntry(2, 0, 3);

            _stateMachine.SendStateMessage(0);
            _stateMachine.CurrentState.ShouldBe(1);

            _stateMachine.SendStateMessage(0);
            _stateMachine.CurrentState.ShouldBe(2);

            _stateMachine.SendStateMessage(0);
            _stateMachine.CurrentState.ShouldBe(3);
        }

        #endregion
    }
}
