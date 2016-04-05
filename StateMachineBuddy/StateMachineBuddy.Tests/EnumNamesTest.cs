using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StateMachineBuddy;

namespace StateMachineBuddy.Tests
{
	[TestFixture]
	public class EnumNamesTest
	{
		private enum simple
		{
			zero = 0,
			one = 1,
			two = 2,
		}

		private enum complex
		{
			twotwo =2,
			three = 3,
			four = 4,
		}

		[Test]
		public void NamesMatch()
		{
			var stm = new StateMachine();
			stm.Set(Enum.GetValues(typeof(simple)).Length, 0, 0, 0);

			stm.SetNames(typeof (simple), true);

			Assert.AreEqual("zero", stm.GetStateName(0));
		}

		[Test]
		public void NamesMatch1()
		{
			var stm = new StateMachine();
			stm.Set(Enum.GetValues(typeof(simple)).Length, 0, 0, 0);

			stm.SetNames(typeof(simple), true);

			Assert.AreEqual("zero", stm.GetStateName(0));
			Assert.AreEqual("one", stm.GetStateName(1));
			Assert.AreEqual("two", stm.GetStateName(2));
		}

		[Test]
		public void NamesMatch3()
		{
			var stm = new StateMachine();
			stm.Set(Enum.GetValues(typeof(simple)).Length, 0, 0, 0);

			stm.SetNames(typeof(simple), true);

			Assert.AreEqual("zero", stm.GetStateName((int)simple.zero));
			Assert.AreEqual("one", stm.GetStateName((int)simple.one));
			Assert.AreEqual("two", stm.GetStateName((int)simple.two));
		}

		[Test]
		public void MessagesMatch()
		{
			var stm = new StateMachine();
			stm.Set(0, Enum.GetValues(typeof(simple)).Length, 0, 0);

			stm.SetNames(typeof(simple), false);

			Assert.AreEqual("zero", stm.GetMessageName((int)simple.zero));
			Assert.AreEqual("one", stm.GetMessageName((int)simple.one));
			Assert.AreEqual("two", stm.GetMessageName((int)simple.two));
		}

		[Test]
		public void ComplexMatch()
		{
			var stm = new StateMachine();
			stm.Set((int)complex.four + 1, 0, 0, 0);

			stm.SetNames(typeof(simple), true);
			stm.SetNames(typeof(complex), true);

			Assert.AreEqual("zero", stm.GetStateName((int)simple.zero));
		}

		[Test]
		public void ComplexMatch1()
		{
			var stm = new StateMachine();
			stm.Set((int)complex.four + 1, 0, 0, 0);

			stm.SetNames(typeof(simple), true);
			stm.SetNames(typeof(complex), true);

			Assert.AreEqual("zero", stm.GetStateName((int)simple.zero));
			Assert.AreEqual("one", stm.GetStateName((int)simple.one));
			Assert.AreEqual("twotwo", stm.GetStateName((int)simple.two));
			Assert.AreEqual("twotwo", stm.GetStateName((int)complex.twotwo));
		}

		[Test]
		public void ComplexMatch2()
		{
			var stm = new StateMachine();
			stm.Set((int)complex.four + 1, 0, 0, 0);

			stm.SetNames(typeof(simple), true);
			stm.SetNames(typeof(complex), true);

			Assert.AreEqual("zero", stm.GetStateName((int)simple.zero));
			Assert.AreEqual("one", stm.GetStateName((int)simple.one));
			Assert.AreEqual("twotwo", stm.GetStateName((int)simple.two));
			Assert.AreEqual("three", stm.GetStateName((int)complex.three));
			Assert.AreEqual("four", stm.GetStateName((int)complex.four));
		}
	}
}
