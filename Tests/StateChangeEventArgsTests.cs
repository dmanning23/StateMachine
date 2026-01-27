using NUnit.Framework;
using Shouldly;
using StateMachineBuddy;

namespace Tests
{
    [TestFixture]
    public class StateChangeEventArgsTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_WithStrings_SetsProperties()
        {
            var args = new StateChangeEventArgs<string>("OldState", "NewState");

            args.OldState.ShouldBe("OldState");
            args.NewState.ShouldBe("NewState");
        }

        [Test]
        public void Constructor_WithInts_SetsProperties()
        {
            var args = new StateChangeEventArgs<int>(1, 2);

            args.OldState.ShouldBe(1);
            args.NewState.ShouldBe(2);
        }

        [Test]
        public void Constructor_WithSameValues_SetsProperties()
        {
            var args = new StateChangeEventArgs<string>("Same", "Same");

            args.OldState.ShouldBe("Same");
            args.NewState.ShouldBe("Same");
        }

        [Test]
        public void Constructor_WithNullStrings_SetsProperties()
        {
            var args = new StateChangeEventArgs<string>(null, null);

            args.OldState.ShouldBeNull();
            args.NewState.ShouldBeNull();
        }

        #endregion

        #region Property Tests

        [Test]
        public void OldState_CanBeSet()
        {
            var args = new StateChangeEventArgs<string>("A", "B");

            args.OldState = "C";

            args.OldState.ShouldBe("C");
        }

        [Test]
        public void NewState_CanBeSet()
        {
            var args = new StateChangeEventArgs<string>("A", "B");

            args.NewState = "D";

            args.NewState.ShouldBe("D");
        }

        #endregion

        #region Generic Type Tests

        [Test]
        public void GenericType_WorksWithEnums()
        {
            var args = new StateChangeEventArgs<TestState>(TestState.one, TestState.two);

            args.OldState.ShouldBe(TestState.one);
            args.NewState.ShouldBe(TestState.two);
        }

        [Test]
        public void GenericType_WorksWithCustomClass()
        {
            var old = new CustomState { Name = "Old" };
            var newState = new CustomState { Name = "New" };

            var args = new StateChangeEventArgs<CustomState>(old, newState);

            args.OldState.Name.ShouldBe("Old");
            args.NewState.Name.ShouldBe("New");
        }

        private class CustomState
        {
            public string Name { get; set; }
        }

        #endregion
    }
}
