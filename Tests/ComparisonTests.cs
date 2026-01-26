using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateMachineBuddy;

namespace Tests
{
    [TestFixture]
    public class ComparisonTests
    {
        StringStateMachine _states;

        IntStateMachine _stateMachine;

        [SetUp]
        public void Setup()
        {
            _states = new StringStateMachine();

            _stateMachine = new IntStateMachine();
            _stateMachine.Set(typeof(TestState), typeof(TestMessage), 0);
            _stateMachine.SetEntry((int)TestState.one, (int)TestMessage.one, (int)TestState.two);
            _stateMachine.SetEntry((int)TestState.one, (int)TestMessage.two, (int)TestState.three);
            _stateMachine.SetEntry((int)TestState.two, (int)TestMessage.one, (int)TestState.three);
            _stateMachine.SetEntry((int)TestState.two, (int)TestMessage.two, (int)TestState.four);

            _states.AddStates(typeof(TestState));
            _states.AddMessages(typeof(TestMessage));
            _states.SetInitialState("one");
            _states.Set(TestState.one.ToString(), TestMessage.one.ToString(), TestState.two.ToString());
            _states.Set(TestState.one.ToString(), TestMessage.two.ToString(), TestState.three.ToString());
            _states.Set(TestState.two.ToString(), TestMessage.one.ToString(), TestState.three.ToString());
            _states.Set(TestState.two.ToString(), TestMessage.two.ToString(), TestState.four.ToString());
        }

        [Test]
        public void WhichIsFaster()
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 10000000; i++)
            {
                _stateMachine.SendStateMessage(0);
                _stateMachine.SendStateMessage(1);
            }
            stopwatch.Stop();
            Console.WriteLine($"StateMachine elapsed time: {stopwatch.ElapsedMilliseconds}");

            var stopwatch1 = Stopwatch.StartNew();
            for (var i = 0; i < 10000000; i++)
            {
                _states.SendStateMessage("0");
                _states.SendStateMessage("1");
            }
            stopwatch1.Stop();
            Console.WriteLine($"HybridStateMachine elapsed time: {stopwatch1.ElapsedMilliseconds}");
        }
    }
}
