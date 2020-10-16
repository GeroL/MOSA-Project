// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework.IR;

namespace Mosa.Compiler.Framework.Transform.Auto.IR.ConstantFolding
{
	/// <summary>
	/// SubCarryIn32NoCarry
	/// </summary>
	public sealed class SubCarryIn32NoCarry : BaseTransformation
	{
		public SubCarryIn32NoCarry() : base(IRInstruction.SubCarryIn32)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!context.Operand3.IsResolvedConstant)
				return false;

			if (context.Operand3.ConstantUnsigned64 != 0)
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1;
			var t2 = context.Operand2;

			context.SetInstruction(IRInstruction.Sub32, result, t1, t2);
		}
	}
}
