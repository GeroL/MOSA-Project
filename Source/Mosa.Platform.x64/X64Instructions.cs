// Copyright (c) MOSA Project. Licensed under the New BSD License.

// This code was generated by an automated template.

using Mosa.Compiler.Framework;
using System.Collections.Generic;

namespace Mosa.Platform.x64
{
	/// <summary>
	/// X64 Instruction Map
	/// </summary>
	public static class X64Instructions
	{
		public static readonly List<BaseInstruction> List = new List<BaseInstruction> {
			X64.Adc64,
			X64.AdcConst64,
			X64.Add64,
			X64.AddConst64,
			X64.Addsd,
			X64.Addss,
			X64.And64,
			X64.AndConst64,
			X64.Break,
			X64.Btr64,
			X64.BtrConst64,
			X64.Bt64,
			X64.BtConst64,
			X64.Bts64,
			X64.BtsConst64,
			X64.Call,
			X64.CallStatic,
			X64.CallReg,
			X64.Cdq,
			X64.Cli,
			X64.Cmp64,
			X64.CmpConst64,
			X64.CmpXChgLoad64,
			X64.Comisd,
			X64.Comiss,
			X64.CpuId,
			X64.Cvtsd2ss,
			X64.Cvtsi2sd,
			X64.Cvtsi2ss,
			X64.Cvtss2sd,
			X64.Cvttsd2si,
			X64.Cvttss2si,
			X64.Dec64,
			X64.Div64,
			X64.Divsd,
			X64.Divss,
			X64.JmpFar,
			X64.Hlt,
			X64.IDiv64,
			X64.IMul64,
			X64.In8,
			X64.In16,
			X64.In32,
			X64.InConst8,
			X64.InConst16,
			X64.InConst32,
			X64.Inc64,
			X64.Int,
			X64.Invlpg,
			X64.IRetd,
			X64.Jmp,
			X64.JmpStatic,
			X64.JmpReg,
			X64.Lea64,
			X64.Leave,
			X64.Lgdt,
			X64.Lidt,
			X64.Lock,
			X64.MovLoadSeg64,
			X64.MovStoreSeg64,
			X64.Mov64,
			X64.MovConst64,
			X64.Movaps,
			X64.MovapsLoad,
			X64.MovCRLoad64,
			X64.MovCRStore64,
			X64.Movd,
			X64.MovLoad8,
			X64.MovLoad16,
			X64.MovLoad32,
			X64.MovLoad64,
			X64.Movsd,
			X64.MovsdLoad,
			X64.MovsdStore,
			X64.Movss,
			X64.MovssLoad,
			X64.MovssStore,
			X64.MovStore8,
			X64.MovStore16,
			X64.MovStore32,
			X64.MovStore64,
			X64.Movsx8To64,
			X64.Movsx16To64,
			X64.MovsxLoad8,
			X64.MovsxLoad16,
			X64.MovsxLoad32,
			X64.Movups,
			X64.MovupsLoad,
			X64.MovupsStore,
			X64.Movzx8To64,
			X64.Movzx16To64,
			X64.MovzxLoad8,
			X64.MovzxLoad16,
			X64.MovzxLoad32,
			X64.Mul64,
			X64.Mulsd,
			X64.Mulss,
			X64.Neg64,
			X64.Nop,
			X64.Not64,
			X64.Or64,
			X64.OrConst64,
			X64.Out8,
			X64.Out16,
			X64.Out32,
			X64.OutConst8,
			X64.OutConst16,
			X64.OutConst32,
			X64.Pause,
			X64.Pextrd,
			X64.Pop64,
			X64.Popad,
			X64.Push64,
			X64.PushConst64,
			X64.Pushad,
			X64.PXor,
			X64.Rcr64,
			X64.RcrConst64,
			X64.RcrConstOne64,
			X64.Rep,
			X64.Ret,
			X64.Roundsd,
			X64.Roundss,
			X64.Sar64,
			X64.SarConst64,
			X64.SarConstOne64,
			X64.Sbb64,
			X64.SbbConst64,
			X64.Shl64,
			X64.ShlConst64,
			X64.ShlConstOne64,
			X64.Shld64,
			X64.ShldConst64,
			X64.Shr64,
			X64.ShrConst64,
			X64.ShrConstOne64,
			X64.Shrd64,
			X64.ShrdConst64,
			X64.Sti,
			X64.Stos,
			X64.Sub64,
			X64.SubConst64,
			X64.Subsd,
			X64.Subss,
			X64.Test64,
			X64.TestConst64,
			X64.Ucomisd,
			X64.Ucomiss,
			X64.XAddLoad64,
			X64.XChg64,
			X64.XChgLoad64,
			X64.Xor64,
			X64.XorConst64,
			X64.BranchOverflow,
			X64.BranchNoOverflow,
			X64.BranchCarry,
			X64.BranchUnsignedLessThan,
			X64.BranchUnsignedGreaterOrEqual,
			X64.BranchNoCarry,
			X64.BranchEqual,
			X64.BranchZero,
			X64.BranchNotEqual,
			X64.BranchNotZero,
			X64.BranchUnsignedLessOrEqual,
			X64.BranchUnsignedGreaterThan,
			X64.BranchSigned,
			X64.BranchNotSigned,
			X64.BranchParity,
			X64.BranchNoParity,
			X64.BranchLessThan,
			X64.BranchGreaterOrEqual,
			X64.BranchLessOrEqual,
			X64.BranchGreaterThan,
			X64.SetByteIfOverflow,
			X64.SetByteIfNoOverflow,
			X64.SetByteIfCarry,
			X64.SetByteIfUnsignedLessThan,
			X64.SetByteIfUnsignedGreaterOrEqual,
			X64.SetByteIfNoCarry,
			X64.SetByteIfEqual,
			X64.SetByteIfZero,
			X64.SetByteIfNotEqual,
			X64.SetByteIfNotZero,
			X64.SetByteIfUnsignedLessOrEqual,
			X64.SetByteIfUnsignedGreaterThan,
			X64.SetByteIfSigned,
			X64.SetByteIfNotSigned,
			X64.SetByteIfParity,
			X64.SetByteIfNoParity,
			X64.SetByteIfLessThan,
			X64.SetByteIfGreaterOrEqual,
			X64.SetByteIfLessOrEqual,
			X64.SetByteIfGreaterThan,
			X64.CMovOverflow64,
			X64.CMovNoOverflow64,
			X64.CMovCarry64,
			X64.CMovUnsignedLessThan64,
			X64.CMovUnsignedGreaterOrEqual64,
			X64.CMovNoCarry64,
			X64.CMovEqual64,
			X64.CMovZero64,
			X64.CMovNotEqual64,
			X64.CMovNotZero64,
			X64.CMovUnsignedLessOrEqual64,
			X64.CMovUnsignedGreaterThan64,
			X64.CMovSigned64,
			X64.CMovNotSigned64,
			X64.CMovParity64,
			X64.CMovNoParity64,
			X64.CMovLessThan64,
			X64.CMovGreaterOrEqual64,
			X64.CMovLessOrEqual64,
			X64.CMovGreaterThan64,
		};
	}
}
