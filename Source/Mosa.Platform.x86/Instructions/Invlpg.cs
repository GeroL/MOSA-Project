// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x86.Instructions
{
	/// <summary>
	/// Invlpg
	/// </summary>
	/// <seealso cref="Mosa.Platform.x86.X86Instruction" />
	public sealed class Invlpg : X86Instruction
	{
		internal Invlpg()
			: base(0, 1)
		{
		}

		public override bool HasUnspecifiedSideEffect { get { return true; } }

		public override void Emit(InstructionNode node, OpcodeEncoder opcodeEncoder)
		{
			System.Diagnostics.Debug.Assert(node.ResultCount == 0);
			System.Diagnostics.Debug.Assert(node.OperandCount == 1);

			if (node.Operand1.IsCPURegister)
			{
				opcodeEncoder.Append8Bits(0x0F);
				opcodeEncoder.Append8Bits(0x01);
				opcodeEncoder.Append2Bits(0b00);
				opcodeEncoder.Append3Bits(0b010);
				opcodeEncoder.Append3Bits(node.Operand1.Register.RegisterCode);
				return;
			}

			if (node.Operand1.IsConstant)
			{
				opcodeEncoder.Append8Bits(0x0F);
				opcodeEncoder.Append8Bits(0x01);
				opcodeEncoder.Append2Bits(0b00);
				opcodeEncoder.Append3Bits(0b010);
				opcodeEncoder.Append3Bits(0b101);
				opcodeEncoder.Append32BitImmediate(node.Operand1);
				return;
			}

			throw new Compiler.Common.Exceptions.CompilerException("Invalid Opcode");
		}
	}
}
