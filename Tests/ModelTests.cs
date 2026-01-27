using NUnit.Framework;
using Shouldly;
using StateMachineBuddy;

namespace Tests
{
    [TestFixture]
    public class StateMachineModelTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Default_InitializesEmptyLists()
        {
            var model = new StateMachineModel();

            model.StateNames.ShouldNotBeNull();
            model.StateNames.Count.ShouldBe(0);
            model.MessageNames.ShouldNotBeNull();
            model.MessageNames.Count.ShouldBe(0);
            model.States.ShouldNotBeNull();
            model.States.Count.ShouldBe(0);
            model.Initial.ShouldBeNull();
        }

        [Test]
        public void Constructor_FromStateMachine_CopiesData()
        {
            var stateMachine = new StringStateMachine();
            stateMachine.AddStates(new[] { "A", "B" });
            stateMachine.AddMessages(new[] { "X" });
            stateMachine.SetInitialState("A");
            stateMachine.Set("A", "X", "B");

            var model = new StateMachineModel(null, stateMachine);

            model.Initial.ShouldBe("A");
            model.StateNames.Count.ShouldBe(2);
            model.MessageNames.Count.ShouldBe(1);
        }

        #endregion

        #region Property Tests

        [Test]
        public void Initial_CanBeSet()
        {
            var model = new StateMachineModel();

            model.Initial = "StartState";

            model.Initial.ShouldBe("StartState");
        }

        [Test]
        public void StateNames_CanAddItems()
        {
            var model = new StateMachineModel();

            model.StateNames.Add("State1");
            model.StateNames.Add("State2");

            model.StateNames.Count.ShouldBe(2);
        }

        [Test]
        public void MessageNames_CanAddItems()
        {
            var model = new StateMachineModel();

            model.MessageNames.Add("Msg1");
            model.MessageNames.Add("Msg2");

            model.MessageNames.Count.ShouldBe(2);
        }

        [Test]
        public void States_CanAddItems()
        {
            var model = new StateMachineModel();

            model.States.Add(new StateModel { Name = "A" });
            model.States.Add(new StateModel { Name = "B" });

            model.States.Count.ShouldBe(2);
        }

        #endregion
    }

    [TestFixture]
    public class StateModelTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Default_InitializesEmptyTransitions()
        {
            var model = new StateModel();

            model.Transitions.ShouldNotBeNull();
            model.Transitions.Count.ShouldBe(0);
            model.Name.ShouldBeNull();
        }

        [Test]
        public void Constructor_FromState_CopiesTransitions()
        {
            var state = new State();
            state.StateChanges["X"] = "B";
            state.StateChanges["Y"] = "C";

            var model = new StateModel("A", state);

            model.Name.ShouldBe("A");
            model.Transitions.Count.ShouldBe(2);
        }

        [Test]
        public void Constructor_FromState_ExcludesSelfTransitions()
        {
            var state = new State();
            state.StateChanges["X"] = "B";
            state.StateChanges["Y"] = "A"; // Self transition

            var model = new StateModel("A", state);

            model.Transitions.Count.ShouldBe(1);
            model.Transitions[0].Message.ShouldBe("X");
        }

        #endregion

        #region Property Tests

        [Test]
        public void Name_CanBeSet()
        {
            var model = new StateModel();

            model.Name = "TestState";

            model.Name.ShouldBe("TestState");
        }

        [Test]
        public void Transitions_CanAddItems()
        {
            var model = new StateModel();

            model.Transitions.Add(new StateChangeModel("Msg", "Target"));

            model.Transitions.Count.ShouldBe(1);
        }

        #endregion
    }

    [TestFixture]
    public class StateChangeModelTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Default_InitializesNullProperties()
        {
            var model = new StateChangeModel();

            model.Message.ShouldBeNull();
            model.TargetState.ShouldBeNull();
        }

        [Test]
        public void Constructor_WithParameters_SetsProperties()
        {
            var model = new StateChangeModel("TestMessage", "TestTarget");

            model.Message.ShouldBe("TestMessage");
            model.TargetState.ShouldBe("TestTarget");
        }

        #endregion

        #region Property Tests

        [Test]
        public void Message_CanBeSet()
        {
            var model = new StateChangeModel();

            model.Message = "MyMessage";

            model.Message.ShouldBe("MyMessage");
        }

        [Test]
        public void TargetState_CanBeSet()
        {
            var model = new StateChangeModel();

            model.TargetState = "MyTarget";

            model.TargetState.ShouldBe("MyTarget");
        }

        #endregion
    }
}
