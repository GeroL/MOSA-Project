﻿/*
 * (c) 2010 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Simon Wollwage (rootnode) <rootnode@mosa-project.org>
 *
 */

using System;
using MbUnit.Framework;

using Mosa.Test.System;
using Mosa.Test.Collection;

namespace Mosa.Test.Cases.CIL
{
	[TestFixture]
	public class FibonacciFixture : TestCompilerAdapter
	{

		public FibonacciFixture()
		{
			settings.AddReference("Mosa.Test.Collection.dll");
		}

		[Test]
		public void Fibonacci([I4Small] int value)
		{
			Assert.AreEqual(FibonacciTests.Fibonacci(value), Run<int>("Mosa.Test.Collection", "FibonacciTests", "Fibonacci", value));
		}
	}
}