// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework.IR;

namespace Mosa.Compiler.Framework.Transform.Auto.IR.ConstantFolding
{
	/// <summary>
	/// MulUnsigned64x2
	/// </summary>
	public sealed class MulUnsigned64x2 : BaseTransformation
	{
		public MulUnsigned64x2() : base(IRInstruction.MulUnsigned64)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!context.Operand1.IsVirtualRegister)
				return false;

			if (context.Operand1.Definitions.Count != 1)
				return false;

			if (context.Operand1.Definitions[0].Instruction != IRInstruction.MulUnsigned64)
				return false;

			if (!IsResolvedConstant(context.Operand1.Definitions[0].Operand2))
				return false;

			if (!IsResolvedConstant(context.Operand2))
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1.Definitions[0].Operand1;
			var t2 = context.Operand1.Definitions[0].Operand2;
			var t3 = context.Operand2;

			var e1 = transformContext.CreateConstant(MulUnsigned64(To64(t2), To64(t3)));

			context.SetInstruction(IRInstruction.MulUnsigned64, result, t1, e1);
		}
	}

	/// <summary>
	/// MulUnsigned64x2_v1
	/// </summary>
	public sealed class MulUnsigned64x2_v1 : BaseTransformation
	{
		public MulUnsigned64x2_v1() : base(IRInstruction.MulUnsigned64)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!context.Operand2.IsVirtualRegister)
				return false;

			if (context.Operand2.Definitions.Count != 1)
				return false;

			if (context.Operand2.Definitions[0].Instruction != IRInstruction.MulUnsigned64)
				return false;

			if (!IsResolvedConstant(context.Operand2.Definitions[0].Operand2))
				return false;

			if (!IsResolvedConstant(context.Operand1))
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1;
			var t2 = context.Operand2.Definitions[0].Operand1;
			var t3 = context.Operand2.Definitions[0].Operand2;

			var e1 = transformContext.CreateConstant(MulUnsigned64(To64(t3), To64(t1)));

			context.SetInstruction(IRInstruction.MulUnsigned64, result, t2, e1);
		}
	}

	/// <summary>
	/// MulUnsigned64x2_v2
	/// </summary>
	public sealed class MulUnsigned64x2_v2 : BaseTransformation
	{
		public MulUnsigned64x2_v2() : base(IRInstruction.MulUnsigned64)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!context.Operand1.IsVirtualRegister)
				return false;

			if (context.Operand1.Definitions.Count != 1)
				return false;

			if (context.Operand1.Definitions[0].Instruction != IRInstruction.MulUnsigned64)
				return false;

			if (!IsResolvedConstant(context.Operand1.Definitions[0].Operand1))
				return false;

			if (!IsResolvedConstant(context.Operand2))
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1.Definitions[0].Operand1;
			var t2 = context.Operand1.Definitions[0].Operand2;
			var t3 = context.Operand2;

			var e1 = transformContext.CreateConstant(MulUnsigned64(To64(t1), To64(t3)));

			context.SetInstruction(IRInstruction.MulUnsigned64, result, t2, e1);
		}
	}

	/// <summary>
	/// MulUnsigned64x2_v3
	/// </summary>
	public sealed class MulUnsigned64x2_v3 : BaseTransformation
	{
		public MulUnsigned64x2_v3() : base(IRInstruction.MulUnsigned64)
		{
		}

		public override bool Match(Context context, TransformContext transformContext)
		{
			if (!context.Operand2.IsVirtualRegister)
				return false;

			if (context.Operand2.Definitions.Count != 1)
				return false;

			if (context.Operand2.Definitions[0].Instruction != IRInstruction.MulUnsigned64)
				return false;

			if (!IsResolvedConstant(context.Operand2.Definitions[0].Operand1))
				return false;

			if (!IsResolvedConstant(context.Operand1))
				return false;

			return true;
		}

		public override void Transform(Context context, TransformContext transformContext)
		{
			var result = context.Result;

			var t1 = context.Operand1;
			var t2 = context.Operand2.Definitions[0].Operand1;
			var t3 = context.Operand2.Definitions[0].Operand2;

			var e1 = transformContext.CreateConstant(MulUnsigned64(To64(t2), To64(t1)));

			context.SetInstruction(IRInstruction.MulUnsigned64, result, t3, e1);
		}
	}
}
