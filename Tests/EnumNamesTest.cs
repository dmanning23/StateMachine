using NUnit.Framework;
using System;
using StateMachineBuddy;
using Shouldly;

namespace Tests
{
    [TestFixture]
    public class EnumNamesTest
    {
        [Test]
        public void LengthsMatch()
        {
            var stm = new IntStateMachine();
            stm.Set(Enum.GetValues(typeof(TestState)).Length,
                Enum.GetValues(typeof(TestMessage)).Length,
                0);
            stm.SetNames(typeof(TestState), true);
            stm.SetNames(typeof(TestMessage), false);

            stm.NumStates.ShouldBe(Enum.GetValues(typeof(TestState)).Length);
            stm.NumMessages.ShouldBe(Enum.GetValues(typeof(TestMessage)).Length);
        }

        [Test]
        public void StatesMatch()
        {
            var stm = new IntStateMachine();
            stm.Set(Enum.GetValues(typeof(TestState)).Length,
                Enum.GetValues(typeof(TestMessage)).Length,
                0);
            stm.SetNames(typeof(TestState), true);
            stm.SetNames(typeof(TestMessage), false);

            stm.GetStateName(0).ShouldBe("one");
            stm.GetStateName(1).ShouldBe("two");
            stm.GetStateName(2).ShouldBe("three");
        }

        [Test]
        public void MessagesMatch()
        {
            var stm = new IntStateMachine();
            stm.Set(Enum.GetValues(typeof(TestState)).Length,
                Enum.GetValues(typeof(TestMessage)).Length,
                0);
            stm.SetNames(typeof(TestState), true);
            stm.SetNames(typeof(TestMessage), false);

            stm.GetMessageName((int)TestMessage.one).ShouldBe("one");
            stm.GetMessageName((int)TestMessage.two).ShouldBe("two");
        }
    }
}
