﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Alex Lyman <mail.alex.lyman@gmail.com>
 *  Simon Wollwage (rootnode) <kintaro@think-in-co.de>
 *  Michael Fröhlich (grover) <michael.ruck@michaelruck.de>
 *  
 */

using System;
using System.Collections.Generic;
using System.Text;
using MbUnit.Framework;
using System.Reflection.Emit;

using Mosa.Test.Runtime.CompilerFramework;

namespace Mosa.Test.Cases.IL
{
	[TestFixture]
	public class ConvI2 : TestCompilerAdapter
	{
		
		[Column(0, 1, 2, sbyte.MinValue, sbyte.MaxValue)]
		[Test]
		public void ConvI2_I1(sbyte a)
		{
			settings.CodeSource = @"
				static class Test { 
					static bool ConvI2_I1(short expect, sbyte a) 
					{ 
						return expect == ((short)a); 
					} 
				}";
			Assert.IsTrue(Run<bool>(string.Empty, "Test", "ConvI2_I1", ((short)a), a));
		}

		[Column(0, 1, 2, short.MinValue, short.MaxValue)]
		[Test]
		public void ConvI2_I2(short a)
		{
			settings.CodeSource = @"
				static class Test { 
					static bool ConvI2_I2(short expect, short a)
					{ 
						return expect == ((short)a); 
					} 
				}";
			Assert.IsTrue(Run<bool>(string.Empty, "Test", "ConvI2_I2", a, a));
		}

		[Column(0, 1, 2, int.MinValue, int.MaxValue)]
		[Test]
		public void ConvI2_I4(int a)
		{
			settings.CodeSource = @"
				static class Test { 
					static bool ConvI2_I4(short expect, int a) 
					{ 
						return expect == ((short)a); 
					} 
				}";
			Assert.IsTrue(Run<bool>(string.Empty, "Test", "ConvI2_I4", ((short)a), a));
		}

		[Column(0, 1, 2, long.MinValue, long.MaxValue)]
		[Test]
		public void ConvI2_I8(long a)
		{
			settings.CodeSource = @"
				static class Test { 
					static bool ConvI2_I8(short expect, long a) 
					{ 
						return expect == ((short)a); 
					} 
				}";
			Assert.IsTrue(Run<bool>(string.Empty, "Test", "ConvI2_I8", ((short)a), a));
		}

		[Column(0.0f, 1.0f, 2.0f, Single.MinValue, Single.MaxValue)]
		[Test]
		public void ConvI2_R4(float a)
		{
			settings.CodeSource = @"
				static class Test 
				{ 
					static bool ConvI2_R4(short expect, float a) 
					{ 
						return expect == ((short)a); 
					} 
				}";
			Assert.IsTrue(Run<bool>(string.Empty, "Test", "ConvI2_R4", ((short)a), a));
		}

		[Column(0.0f, 1.0f, 2.0f, Double.MinValue, Double.MaxValue)]
		[Test]
		public void ConvI2_R8(double a)
		{
			settings.CodeSource = @"
				static class Test { 
					static bool ConvI2_R8(short expect, double a) 
					{ 
						return expect == ((short)a); 
					} 
				}";
			Assert.IsTrue(Run<bool>(string.Empty, "Test", "ConvI2_R8", ((short)a), a));
		}
	}
}