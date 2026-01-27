using NUnit.Framework;
using Shouldly;
using StateMachineBuddy;

namespace Tests
{
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Jumping
    }

    public enum PlayerMessage
    {
        Walk,
        Run,
        Jump,
        Stop,
        Land
    }

    [TestFixture]
    public class EnumStateMachineTests
    {
        private EnumStateMachine<PlayerState, PlayerMessage> _stateMachine;

        [SetUp]
        public void Setup()
        {
            _stateMachine = new EnumStateMachine<PlayerState, PlayerMessage>(PlayerState.Idle);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_SetsInitialState()
        {
            _stateMachine.InitialState.ShouldBe("Idle");
            _stateMachine.CurrentState.ShouldBe("Idle");
            _stateMachine.PrevState.ShouldBe("Idle");
        }

        [Test]
        public void Constructor_AddsAllEnumStates()
        {
            _stateMachine.States.Count.ShouldBe(4);
            _stateMachine.States.ShouldContain("Idle");
            _stateMachine.States.ShouldContain("Walking");
            _stateMachine.States.ShouldContain("Running");
            _stateMachine.States.ShouldContain("Jumping");
        }

        [Test]
        public void Constructor_AddsAllEnumMessages()
        {
            _stateMachine.Messages.ShouldContain("Walk");
            _stateMachine.Messages.ShouldContain("Run");
            _stateMachine.Messages.ShouldContain("Jump");
            _stateMachine.Messages.ShouldContain("Stop");
            _stateMachine.Messages.ShouldContain("Land");
        }

        [Test]
        public void Constructor_DifferentInitialState_SetsCorrectly()
        {
            var machine = new EnumStateMachine<PlayerState, PlayerMessage>(PlayerState.Running);

            machine.InitialState.ShouldBe("Running");
            machine.CurrentState.ShouldBe("Running");
        }

        #endregion

        #region Set Tests

        [Test]
        public void Set_WithEnumValues_DefinesTransition()
        {
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Walk, PlayerState.Walking);

            _stateMachine.StateTable["Idle"].StateChanges["Walk"].ShouldBe("Walking");
        }

        [Test]
        public void Set_MultipleTransitions_AllWork()
        {
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Walk, PlayerState.Walking);
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Run, PlayerState.Running);
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Jump, PlayerState.Jumping);

            _stateMachine.StateTable["Idle"].StateChanges.Count.ShouldBe(3);
        }

        #endregion

        #region SendStateMessage Tests

        [Test]
        public void SendStateMessage_WithEnumTransition_ChangesState()
        {
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Walk, PlayerState.Walking);

            var result = _stateMachine.SendStateMessage("Walk");

            result.ShouldBeTrue();
            _stateMachine.CurrentState.ShouldBe("Walking");
        }

        [Test]
        public void SendStateMessage_ChainedTransitions_Work()
        {
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Walk, PlayerState.Walking);
            _stateMachine.Set(PlayerState.Walking, PlayerMessage.Run, PlayerState.Running);
            _stateMachine.Set(PlayerState.Running, PlayerMessage.Stop, PlayerState.Idle);

            _stateMachine.SendStateMessage("Walk");
            _stateMachine.CurrentState.ShouldBe("Walking");

            _stateMachine.SendStateMessage("Run");
            _stateMachine.CurrentState.ShouldBe("Running");

            _stateMachine.SendStateMessage("Stop");
            _stateMachine.CurrentState.ShouldBe("Idle");
        }

        #endregion

        #region Event Tests

        [Test]
        public void StateChangedEvent_FiresOnTransition()
        {
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Jump, PlayerState.Jumping);

            string oldState = null;
            string newState = null;
            _stateMachine.StateChangedEvent += (sender, args) =>
            {
                oldState = args.OldState;
                newState = args.NewState;
            };

            _stateMachine.SendStateMessage("Jump");

            oldState.ShouldBe("Idle");
            newState.ShouldBe("Jumping");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullStateMachine_WorksCorrectly()
        {
            // Set up a complete player state machine
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Walk, PlayerState.Walking);
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Run, PlayerState.Running);
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Jump, PlayerState.Jumping);
            _stateMachine.Set(PlayerState.Walking, PlayerMessage.Stop, PlayerState.Idle);
            _stateMachine.Set(PlayerState.Walking, PlayerMessage.Run, PlayerState.Running);
            _stateMachine.Set(PlayerState.Walking, PlayerMessage.Jump, PlayerState.Jumping);
            _stateMachine.Set(PlayerState.Running, PlayerMessage.Stop, PlayerState.Idle);
            _stateMachine.Set(PlayerState.Running, PlayerMessage.Jump, PlayerState.Jumping);
            _stateMachine.Set(PlayerState.Jumping, PlayerMessage.Land, PlayerState.Idle);

            // Test sequence
            _stateMachine.CurrentState.ShouldBe("Idle");

            _stateMachine.SendStateMessage("Walk");
            _stateMachine.CurrentState.ShouldBe("Walking");

            _stateMachine.SendStateMessage("Run");
            _stateMachine.CurrentState.ShouldBe("Running");

            _stateMachine.SendStateMessage("Jump");
            _stateMachine.CurrentState.ShouldBe("Jumping");

            _stateMachine.SendStateMessage("Land");
            _stateMachine.CurrentState.ShouldBe("Idle");
        }

        [Test]
        public void ResetToInitialState_Works()
        {
            _stateMachine.Set(PlayerState.Idle, PlayerMessage.Run, PlayerState.Running);
            _stateMachine.SendStateMessage("Run");

            _stateMachine.ResetToInitialState();

            _stateMachine.CurrentState.ShouldBe("Idle");
        }

        [Test]
        public void ForceState_Works()
        {
            _stateMachine.ForceState("Jumping");

            _stateMachine.CurrentState.ShouldBe("Jumping");
            _stateMachine.PrevState.ShouldBe("Idle");
        }

        #endregion
    }
}
