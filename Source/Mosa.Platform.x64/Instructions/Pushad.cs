// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x64.Instructions
{
	/// <summary>
	/// Pushad
	/// </summary>
	/// <seealso cref="Mosa.Platform.x64.X64Instruction" />
	public sealed class Pushad : X64Instruction
	{
		internal Pushad()
			: base(0, 0)
		{
		}

		public override void Emit(InstructionNode node, OpcodeEncoder opcodeEncoder)
		{
			System.Diagnostics.Debug.Assert(node.ResultCount == 0);
			System.Diagnostics.Debug.Assert(node.OperandCount == 0);

			opcodeEncoder.Append8Bits(0x60);
		}
	}
}
