// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x86.Instructions
{
	/// <summary>
	/// TestConst32
	/// </summary>
	/// <seealso cref="Mosa.Platform.x86.X86Instruction" />
	public sealed class TestConst32 : X86Instruction
	{
		public static readonly LegacyOpCode LegacyOpcode = new LegacyOpCode(new byte[] { 0xF7 } , 0x00);

		internal TestConst32()
			: base(0, 2)
		{
		}

		public override bool IsZeroFlagModified { get { return true; } }

		public override bool IsCarryFlagCleared { get { return true; } }

		public override bool IsCarryFlagModified { get { return true; } }

		public override bool IsSignFlagModified { get { return true; } }

		public override bool IsOverflowFlagCleared { get { return true; } }

		public override bool IsOverflowFlagModified { get { return true; } }

		public override bool IsParityFlagModified { get { return true; } }

		internal override void EmitLegacy(InstructionNode node, X86CodeEmitter emitter)
		{
			System.Diagnostics.Debug.Assert(node.ResultCount == 0);
			System.Diagnostics.Debug.Assert(node.OperandCount == 2);

			emitter.Emit(LegacyOpcode, node.Operand1, node.Operand2);
		}
	}
}

