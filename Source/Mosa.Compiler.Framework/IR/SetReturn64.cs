// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

namespace Mosa.Compiler.Framework.IR
{
	/// <summary>
	/// SetReturn64
	/// </summary>
	/// <seealso cref="Mosa.Compiler.Framework.IR.BaseIRInstruction" />
	public sealed class SetReturn64 : BaseIRInstruction
	{
		public SetReturn64()
			: base(1, 0)
		{
		}

		public override FlowControl FlowControl { get { return FlowControl.Return; } }
	}
}