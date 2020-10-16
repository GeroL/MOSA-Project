// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x64.Instructions
{
	/// <summary>
	/// MovStoreSeg32
	/// </summary>
	/// <seealso cref="Mosa.Platform.x64.X64Instruction" />
	public sealed class MovStoreSeg32 : X64Instruction
	{
		internal MovStoreSeg32()
			: base(1, 1)
		{
		}

		public override void Emit(InstructionNode node, OpcodeEncoder opcodeEncoder)
		{
			System.Diagnostics.Debug.Assert(node.ResultCount == 1);
			System.Diagnostics.Debug.Assert(node.OperandCount == 1);

			opcodeEncoder.SuppressByte(0x40);
			opcodeEncoder.Append4Bits(0b0100);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit((node.Result.Register.RegisterCode >> 3) & 0x1);
			opcodeEncoder.Append1Bit(0b0);
			opcodeEncoder.Append1Bit((node.Operand1.Register.RegisterCode >> 3) & 0x1);
			opcodeEncoder.Append8Bits(0x8E);
			opcodeEncoder.Append2Bits(0b11);
			opcodeEncoder.Append3Bits(node.Result.Register.RegisterCode);
			opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
		}
	}
}
