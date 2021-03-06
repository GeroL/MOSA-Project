// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;

namespace Mosa.Compiler.Framework.Transform.Auto.ConstantFolding
{
	/// <summary>
	/// ZeroExtend8x32
	/// </summary>
	public sealed class ZeroExtend8x32 : BaseTransformation
	{
		public ZeroExtend8x32() : base(IRInstruction.ZeroExtend8x32)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!IsResolvedConstant(context.Operand1))
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1;

			var e1 = transformContext.CreateConstant(To32(ToByte(t1)));

			context.SetInstruction(IRInstruction.Move32, result, e1);
		}
	}
}
