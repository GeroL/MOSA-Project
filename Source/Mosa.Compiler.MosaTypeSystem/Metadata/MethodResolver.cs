using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Mosa.Compiler.Common.Exceptions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mosa.Compiler.MosaTypeSystem.Metadata
{
	internal class MethodResolver
	{
		private readonly CLRMetadata _metadata;

		public MethodResolver(CLRMetadata metadata)
		{
			_metadata = metadata;
		}

		public void ResolveMethod(MosaMethod method)
		{
			if (method.UnderlyingObject is null)
				return;

			if (method.IsResolved)
				return;

			//if (method.Resolve != null)
			//{
			//	method.Resolve();
			//	return;
			//}

			var resolver = new GenericArgumentResolver();

			//method.Resolve = () => ResolveBodyInternal(method, resolver);

			bool hasOpening = method.DeclaringType.HasOpenGenericParams;
			var desc = method.GetUnderlyingObject<UnitDesc<MethodDef, MethodSig>>();

			var returnType = _metadata.Loader.GetType(resolver.Resolve(desc.Signature.RetType));
			hasOpening |= returnType.HasOpenGenericParams;

			if (method.DeclaringType.GenericArguments.Count > 0)
			{
				var args = method.DeclaringType.GenericArguments.GetGenericArguments();
				if (!hasOpening)
					foreach (var i in args)
					{
						hasOpening |= i.HasOpenGenericParameter();
						if (hasOpening)
							break;
					}

				resolver.PushTypeGenericArguments(args);
			}

			if (method.GenericArguments.Count > 0)
			{
				var args = method.GenericArguments.GetGenericArguments();
				if (!hasOpening)
					foreach (var i in args)
					{
						hasOpening |= i.HasOpenGenericParameter();
						if (hasOpening)
							break;
					}

				resolver.PushMethodGenericArguments(args);
			}

			using (var mosaMethod = _metadata.Controller.MutateMethod(method))
			{

				var pars = new List<MosaParameter>();

				Debug.Assert(desc.Signature.GetParamCount() + (desc.Signature.HasThis ? 1 : 0) == desc.Definition.Parameters.Count);
				foreach (var param in desc.Definition.Parameters)
				{
					if (!param.IsNormalMethodParameter)
						continue;

					var resolvedType = resolver.Resolve(desc.Signature.Params[param.MethodSigIndex]);
					if (resolvedType == null)
					{
						Debug.WriteLine($"Could not resolve type for: {param.Name} on {method.FullName}");
						continue;
						//var h = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
						//var m = h.First(x => x.Name == desc.Definition.Name);

						//var p = m.GetParameters();
						//var u = desc.Signature.Params.First().GetElementType();
						//var i = u.TryGetPtrSig();
						//var p2 = p.First(x => x.Name == param.Name);
						//resolvedType = resolver.Resolve(new PtrSig(new ));
					}

					var paramType = _metadata.Loader.GetType(resolvedType);

					var parameter = _metadata.Controller.CreateParameter();

					using (var mosaParameter = _metadata.Controller.MutateParameter(parameter))
					{
						mosaParameter.Name = param.Name;
						mosaParameter.ParameterAttributes = (MosaParameterAttributes)(param.ParamDef?.Attributes ?? ParamAttributes.Lcid);
						mosaParameter.ParameterType = paramType;
						mosaParameter.DeclaringMethod = method;
						ResolveCustomAttributes(mosaParameter, param.ParamDef);
					}

					pars.Add(parameter);
					hasOpening |= paramType.HasOpenGenericParams;
				}

				mosaMethod.Signature = new MosaMethodSignature(returnType, pars);

				foreach (var methodImpl in desc.Definition.Overrides)
				{
					Debug.Assert(methodImpl.MethodBody == desc.Definition);
					mosaMethod.Overrides.Add(ResolveMethodOperand(methodImpl.MethodDeclaration, null));
				}

				mosaMethod.HasOpenGenericParams = hasOpening;

				ResolveCustomAttributes(mosaMethod, desc.Definition);
				//ResolveBodyInternal(method, resolver);
				if (desc.Definition.HasBody)
					ResolveBody(desc.Definition, mosaMethod, desc.Definition.Body, resolver);
			}
			method.IsResolved = true;

		}

		private void ResolveBodyInternal(MosaMethod method, GenericArgumentResolver resolver)
		{
			var desc = method.GetUnderlyingObject<UnitDesc<MethodDef, MethodSig>>();
			var definition = desc.Definition;
			if (definition.HasBody)
			{
				using (var mosaMethod = _metadata.Controller.MutateMethod(method))
				{
					ResolveBody(definition, mosaMethod, definition.Body, resolver);
				}
				method.IsResolved = true;
			}
		}

		private static int ResolveOffset(CilBody body, Instruction instruction)
		{
			if (instruction == null)
			{
				instruction = body.Instructions[body.Instructions.Count - 1];
				return (int)(instruction.Offset + instruction.GetSize());
			}
			else
			{
				return (int)instruction.Offset;
			}
		}

		private void ResolveBody(MethodDef methodDef, MosaMethod.Mutator method, CilBody body, GenericArgumentResolver resolver)
		{
			method.LocalVariables.Clear();
			int index = 0;
			foreach (var variable in body.Variables)
			{
				var r = resolver.Resolve(variable.Type);

				if (r is null)
					continue;

				method.LocalVariables.Add(new MosaLocal(
					variable.Name ?? "V_" + index,
					_metadata.Loader.GetType(r),
					variable.Type.IsPinned));
				index++;
			}

			method.ExceptionBlocks.Clear();
			foreach (var eh in body.ExceptionHandlers)
			{
				method.ExceptionBlocks.Add(new MosaExceptionHandler(
					(ExceptionHandlerType)eh.HandlerType,
					ResolveOffset(body, eh.TryStart),
					ResolveOffset(body, eh.TryEnd),
					ResolveOffset(body, eh.HandlerStart),
					ResolveOffset(body, eh.HandlerEnd),
					eh.CatchType == null ? null : _metadata.Loader.GetType(resolver.Resolve(eh.CatchType.ToTypeSig())),
					eh.FilterStart == null ? null : (int?)eh.FilterStart.Offset
				));
			}

			method.MaxStack = methodDef.Body.MaxStack;

			method.Code.Clear();
			for (int i = 0; i < body.Instructions.Count; i++)
			{
				method.Code.Add(ResolveInstruction(methodDef, body, i, resolver));
			}
		}

		private MosaType ResolveTypeOperand(ITypeDefOrRef operand, GenericArgumentResolver resolver)
		{
			return _metadata.Loader.GetType(resolver.Resolve(operand.ToTypeSig()));
		}

		private MosaInstruction ResolveInstruction(MethodDef methodDef, CilBody body, int index, GenericArgumentResolver resolver)
		{
			var instruction = body.Instructions[index];
			int? prev = index == 0 ? null : (int?)body.Instructions[index - 1].Offset;
			int? next = index == body.Instructions.Count - 1 ? null : (int?)body.Instructions[index + 1].Offset;

			object operand = instruction.Operand;

			// Special case: newarr instructions need to have their operand changed now so that the type is a SZArray
			if (instruction.OpCode == OpCodes.Newarr)
			{
				var typeSig = resolver.Resolve(((ITypeDefOrRef)instruction.Operand).ToTypeSig());
				var szArraySig = new SZArraySig(typeSig);
				operand = _metadata.Loader.GetType(szArraySig);
			}
			else if (instruction.Operand is ITypeDefOrRef)
			{
				operand = ResolveTypeOperand((ITypeDefOrRef)instruction.Operand, resolver);
			}
			else if (instruction.Operand is MemberRef memberRef)
			{
				if (memberRef.IsFieldRef)
					operand = ResolveFieldOperand(memberRef, resolver);
				else
					operand = ResolveMethodOperand(memberRef, resolver);
			}
			else if (instruction.Operand is IField)
			{
				operand = ResolveFieldOperand((IField)instruction.Operand, resolver);
			}
			else if (instruction.Operand is IMethod)
			{
				operand = ResolveMethodOperand((IMethod)instruction.Operand, resolver);
			}
			else if (instruction.Operand is Local)
			{
				operand = ((Local)instruction.Operand).Index;
			}
			else if (instruction.Operand is Parameter)
			{
				operand = ((Parameter)instruction.Operand).Index;
			}
			else if (instruction.Operand is Instruction)
			{
				operand = (int)((Instruction)instruction.Operand).Offset;
			}
			else if (instruction.Operand is Instruction[] targets)
			{
				var offsets = new int[targets.Length];
				for (int i = 0; i < offsets.Length; i++)
				{
					offsets[i] = (int)targets[i].Offset;
				}

				operand = offsets;
			}
			else if (instruction.Operand is string)
			{
				operand = _metadata.Cache.GetStringId((string)instruction.Operand);
			}

			ushort code = (ushort)instruction.OpCode.Code;
			if (code > 0xff)    // To match compiler's opcode values
			{
				code = (ushort)(0x100 + (code & 0xff));
			}

			return new MosaInstruction()
			{
				Offset = (int)instruction.Offset,
				OpCode = code,
				Operand = operand,
				Previous = prev,
				Next = next,
				Document = instruction.SequencePoint?.Document.Url,
				StartLine = instruction.SequencePoint?.StartLine ?? 0,
				StartColumn = instruction.SequencePoint?.StartColumn ?? 0,
				EndLine = instruction.SequencePoint?.EndLine ?? 0,
				EndColumn = instruction.SequencePoint?.EndColumn ?? 0,
			};
		}

		private MosaField ResolveFieldOperand(IField operand, GenericArgumentResolver resolver)
		{
			TypeSig declType;
			if (!(operand is FieldDef fieldDef))
			{
				var memberRef = (MemberRef)operand;
				fieldDef = memberRef.ResolveFieldThrow();
				declType = memberRef.DeclaringType.ToTypeSig();
			}
			else
			{
				declType = fieldDef.DeclaringType.ToTypeSig();
			}

			var fieldToken = fieldDef.MDToken;

			var type = _metadata.Loader.GetType(resolver.Resolve(declType));
			foreach (var field in type.Fields)
			{
				var desc = field.Value.GetUnderlyingObject<UnitDesc<FieldDef, FieldSig>>();
				if (desc.Token.Token == fieldToken)
				{
					return field.Value;
				}
			}
			throw new AssemblyLoadException();
		}

		private MosaMethod ResolveMethodOperand(IMethod operand, GenericArgumentResolver resolver)
		{
			if (operand is MethodSpec)
			{
				return _metadata.Loader.LoadGenericMethodInstance((MethodSpec)operand, resolver);
			}
			else if (operand.DeclaringType.TryGetArraySig() != null || operand.DeclaringType.TryGetSZArraySig() != null)
			{
				return ResolveArrayMethod(operand, resolver);
			}

			TypeSig declType;
			if (!(operand is MethodDef methodDef))
			{
				var memberRef = (MemberRef)operand;
				methodDef = memberRef.ResolveMethodThrow();
				declType = memberRef.DeclaringType.ToTypeSig();
			}
			else
			{
				declType = methodDef.DeclaringType.ToTypeSig();
			}

			if (resolver != null)
			{
				declType = resolver.Resolve(declType);
			}

			var methodToken = methodDef.MDToken;

			var type = _metadata.Loader.GetType(declType);
			foreach (var method in type.Methods)
			{
				var desc = method.Value.GetUnderlyingObject<UnitDesc<MethodDef, MethodSig>>();
				if (desc.Token.Token == methodToken)
				{
					return method.Value;
				}
			}

			throw new AssemblyLoadException();
		}

		private MosaMethod ResolveArrayMethod(IMethod method, GenericArgumentResolver resolver)
		{
			var type = _metadata.Loader.GetType(resolver.Resolve(method.DeclaringType.ToTypeSig()));

			if (method.Name == "Get")
				return type.FindMethodByName("Get");
			else if (method.Name == "Set")
				return type.FindMethodByName("Set");
			else if (method.Name == "AddressOf")
				return type.FindMethodByName("AddressOf");
			else if (method.Name == ".ctor")
				return type.FindMethodByName(".ctor");
			else
				throw new AssemblyLoadException();
		}

		private void ResolveCustomAttributes(MosaUnit.MutatorBase unit, IHasCustomAttribute obj)
		{
			if (obj is null)
				return;

			foreach (var attr in obj.CustomAttributes)
			{
				var type = _metadata.Loader.GetType(attr.AttributeType.ToTypeSig());
				var ctor = ((IMethodDefOrRef)attr.Constructor).ResolveMethod();
				MosaMethod mosaCtor = null;
				foreach (var method in type.Methods)
				{
					var desc = method.Value.GetUnderlyingObject<UnitDesc<MethodDef, MethodSig>>();
					if (desc.Token.Token == ctor.MDToken)
					{
						mosaCtor = method.Value;
						break;
					}
				}
				if (mosaCtor == null)
					throw new AssemblyLoadException();

				var values = new MosaCustomAttribute.Argument[attr.ConstructorArguments.Count];
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = ToMosaCAArgument(attr.ConstructorArguments[i]);
				}

				var namedArgs = new MosaCustomAttribute.NamedArgument[attr.NamedArguments.Count];
				for (int i = 0; i < namedArgs.Length; i++)
				{
					var namedArg = attr.NamedArguments[i];
					namedArgs[i] = new MosaCustomAttribute.NamedArgument(namedArg.Name, namedArg.IsField, ToMosaCAArgument(namedArg.Argument));
				}

				unit.CustomAttributes.Add(new MosaCustomAttribute(mosaCtor, values, namedArgs));
			}
		}

		private MosaCustomAttribute.Argument ToMosaCAArgument(CAArgument arg)
		{
			var value = arg.Value;
			if (value is UTF8String)
			{
				value = ((UTF8String)value).String;
			}
			else if (value is TypeSig)
			{
				value = _metadata.Loader.GetType((TypeSig)value);
			}
			else if (value is CAArgument[] valueArray)
			{
				var resultArray = new MosaCustomAttribute.Argument[valueArray.Length];
				for (int i = 0; i < resultArray.Length; i++)
				{
					resultArray[i] = ToMosaCAArgument(valueArray[i]);
				}
			}
			else if (value is IEnumerable<CAArgument> tmp)
			{
				var resultArray = new MosaCustomAttribute.Argument[tmp.Count()];
				for (int i = 0; i < resultArray.Length; i++)
				{
					resultArray[i] = ToMosaCAArgument(tmp.ElementAt(i));
				}
				value = resultArray;
			}

			return new MosaCustomAttribute.Argument(_metadata.Loader.GetType(arg.Type), value);
		}
	}
}
