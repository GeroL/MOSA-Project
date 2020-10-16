// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework.IR;

namespace Mosa.Compiler.Framework.Transform.Auto.IR.ConstantFolding
{
	/// <summary>
	/// SubCarryIn64Outside2
	/// </summary>
	public sealed class SubCarryIn64Outside2 : BaseTransformation
	{
		public SubCarryIn64Outside2() : base(IRInstruction.SubCarryIn64)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!IsResolvedConstant(context.Operand2))
				return false;

			if (!IsResolvedConstant(context.Operand3))
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1;
			var t2 = context.Operand2;
			var t3 = context.Operand3;

			var e1 = transformContext.CreateConstant(Sub64(To64(t2), BoolTo64(To64(t3))));

			context.SetInstruction(IRInstruction.Sub64, result, t1, e1);
		}
	}
}
