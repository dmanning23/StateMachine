using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineBuddy.Tests
{
	[TestFixture]
	public class ComparisonTests
	{
		HybridStateMachine _states;

		StateMachine _stateMachine;

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

			_states.SetStateMachine(_stateMachine);
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
