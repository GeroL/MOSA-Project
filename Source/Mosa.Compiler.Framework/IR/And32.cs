// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.IR
{
	/// <summary>
	/// And32
	/// </summary>
	/// <seealso cref="Mosa.Compiler.Framework.IR.BaseIRInstruction" />
	public sealed class And32 : BaseIRInstruction
	{
		public And32()
			: base(2, 1)
		{
		}

		public override bool IsCommutative { get { return true; } }
	}
}
