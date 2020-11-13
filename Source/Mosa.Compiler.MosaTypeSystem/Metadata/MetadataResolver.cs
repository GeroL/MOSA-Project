// Copyright (c) MOSA Project. Licensed under the New BSD License.

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Mosa.Compiler.Common.Exceptions;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Mosa.Compiler.MosaTypeSystem.Metadata
{
	internal class MetadataResolver
	{
		private readonly CLRMetadata metadata;
		private readonly MethodResolver _methodResolver;

		private readonly Channel<MosaUnit> _channel;
		private readonly ChannelWriter<MosaUnit> _channelWriter;
		private readonly ChannelReader<MosaUnit> _channelReader;


		public MetadataResolver(CLRMetadata metadata)
		{
			this.metadata = metadata;

			_channel = Channel.CreateUnbounded<MosaUnit>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
			_channelWriter = _channel.Writer;
			_channelReader = _channel.Reader;
			_methodResolver = new MethodResolver(metadata);
		}


		//private readonly Queue<MosaUnit> resolveQueue = new Queue<MosaUnit>();
		private readonly Queue<MosaType> arrayResolveQueue = new Queue<MosaType>();

		int count;
		int resolved;
		int oldResolved;
		int errors;

		public void EnqueueForResolve(MosaUnit unit)
		{
			Interlocked.Increment(ref count);
			//resolveQueue.Enqueue(unit);
			_channelWriter.WriteAsync(unit);
		}

		public void EnqueueForArrayResolve(MosaType type)
		{
			if (type is null)
				throw new ArgumentNullException();
			arrayResolveQueue.Enqueue(type);
		}

		public void ResolveThread()
		{
			//use same fix as here: https://github.com/dotnet/machinelearning/pull/5313/files

			//while (_channelReader.WaitToReadAsync().Result)
			//while (_channelReader.TryRead(out var n))
			while (true)
				if (!_channelReader.TryRead(out var n))
				{
					if (count == 0)
						return;

					Thread.Yield();
				}
				else
				{
					try
					{
						Interlocked.Decrement(ref count);
						switch (n)
						{
							case MosaType type: ResolveType(type); break;
							case MosaField field: ResolveField(field); break;
							case MosaMethod method: _methodResolver.ResolveMethod(method); break;
							case MosaProperty property: ResolveProperty(property); break;
							case MosaModule module:
								{
									using (var mosaModule = metadata.Controller.MutateModule(module))
									{
										ResolveCustomAttributes(mosaModule, module.GetUnderlyingObject<UnitDesc<ModuleDef, object>>().Definition);
									}
								}
								break;
						}
						Interlocked.Increment(ref resolved);
					}
					catch (AssemblyLoadException)
					{
						//Type was not really resolved to put it back to the end and continue with next.
						Interlocked.Increment(ref errors);
						EnqueueForResolve(n);
					}
				}
		}

		public void PatchConnections()
		{
			//foreach (var type in metadata.Loader.LoadedUnits.OfType<MosaType>())
			foreach (var type in metadata.TypeSystem.AllTypes)
			{
				var typeDef = type.GetUnderlyingObject<UnitDesc<TypeDef, TypeSig>>()?.Definition;
				if (typeDef is null)
					continue;

				using (var mosaType = metadata.Controller.MutateType(type))
				{
					if (typeDef.BaseType != null)
					{
						mosaType.BaseType = metadata.Loader.GetType(typeDef.BaseType.ToTypeSig());
					}

					if (typeDef.DeclaringType != null)
					{
						mosaType.DeclaringType = metadata.Loader.GetType(typeDef.DeclaringType.ToTypeSig());
					}

					if (typeDef.IsEnum)
					{
						mosaType.ElementType = metadata.Loader.GetType(typeDef.GetEnumUnderlyingType());
					}

					if (typeDef.BaseType != null)
					{
						ResolveInterfacesInBaseTypes(mosaType, type.BaseType);
					}

					foreach (var iface in typeDef.Interfaces)
					{
						var t = metadata.Loader.GetType(iface.Interface.ToTypeSig());

						mosaType.Interfaces[t.Name] = t;
					}
				}
			}

			foreach (var type in metadata.TypeSystem.AllTypes)
			{
				var typeDef = type.GetUnderlyingObject<UnitDesc<TypeDef, TypeSig>>()?.Definition;
				if (typeDef is null || typeDef.Interfaces.Count == type.Interfaces.Count)
					continue;

				using (var mosaType = metadata.Controller.MutateType(type))
				{
					foreach (var iface in typeDef.Interfaces)
					{
						var sig = iface.Interface.ToTypeSig();
						var t = metadata.Loader.GetType(sig);
						//var r = metadata.TypeSystem.GetTypeByName(sig.TypeName);
						mosaType.Interfaces[t.Name] = t;
					}
				}
			}
		}

		private void StartResolving()
		{
			var t1 = new Thread(new ThreadStart(ResolveThread));
			t1.Start();

			while (count > 0)
			{
				Task.Delay(1000).Wait();
				Debug.WriteLine($"Resolved {resolved} {resolved - oldResolved}/s Queue: {count} Errors: {errors}");
				oldResolved = resolved;
			}

			t1.Join();
		}

		public void Resolve()
		{
			//Setup resolving queue 
			foreach (var unit in metadata.Loader.LoadedUnits)
				EnqueueForResolve(unit);

			StartResolving();

			foreach (var module in metadata.Cache.Modules.Values)
			{
				var moduleDef = module.GetUnderlyingObject<UnitDesc<ModuleDef, object>>().Definition;
				if (moduleDef.EntryPoint != null)
				{
					using (var mosaModule = metadata.Controller.MutateModule(module))
					{
						mosaModule.EntryPoint = metadata.Cache.GetMethodByToken(new ScopedToken(moduleDef, moduleDef.EntryPoint.MDToken));
					}
				}
			}

			PatchConnections();

			while (arrayResolveQueue.Count > 0)
			{
				var type = arrayResolveQueue.Dequeue();
				ResolveSZArray(type);
			}
			Debug.Assert(metadata.TypeSystem.AllTypes.Any(x => x.FullName.EndsWith("ICollection`1")));

			//foreach (var method in metadata.Loader.LoadedUnits.OfType<MosaMethod>())
			//{
			//	EnqueueForResolve(method);
			//}

			//foreach (var method in metadata.TypeSystem.AllTypes.SelectMany(x=> x.Methods))
			//{
			//	EnqueueForResolve(method.Value);
			//}

			//StartResolving();
			//var tmp = metadata.TypeSystem.AllTypes.Where(x => x.IsInterface
			//&& x.GetUnderlyingObject<UnitDesc<TypeDef, TypeSig>>().Definition.Interfaces.Count > x.Interfaces.Count)
			// .ToList();
		}

		private void ResolveInterfacesInBaseTypes(MosaType.Mutator mosaType, MosaType baseType)
		{
			foreach (var iface in baseType.Interfaces)
			{
				if (mosaType.Interfaces.ContainsKey(iface.Key))
					continue;

				mosaType.Interfaces.Add(iface);
			}

			if (baseType.BaseType != null)
			{
				ResolveInterfacesInBaseTypes(mosaType, baseType.BaseType);
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
				value = metadata.Loader.GetType((TypeSig)value);
			}
			else if (value is CAArgument[] valueArray)
			{
				var resultArray = new MosaCustomAttribute.Argument[valueArray.Length];
				for (int i = 0; i < resultArray.Length; i++)
				{
					resultArray[i] = ToMosaCAArgument(valueArray[i]);
				}
			}
			else if(value is IEnumerable<CAArgument> tmp)
			{
				var resultArray = new MosaCustomAttribute.Argument[tmp.Count()];
				for (int i = 0; i < resultArray.Length; i++)
				{
					resultArray[i] = ToMosaCAArgument(tmp.ElementAt(i));
				}
				value = resultArray;
			}

			return new MosaCustomAttribute.Argument(metadata.Loader.GetType(arg.Type), value);
		}


		private void ResolveType(MosaType type)
		{
			if (metadata.Controller.HasType(type))
				return;

			var resolver = new GenericArgumentResolver();

			var srcType = type;
			if (type.GenericArguments.Count > 0)
			{
				resolver.PushTypeGenericArguments(type.GenericArguments.GetGenericArguments());
				srcType = type.ElementType;
				Debug.Assert(srcType != null);
			}

			using (var mosaType = metadata.Controller.MutateType(type))
			{
				if (srcType.BaseType != null)
				{
					mosaType.BaseType = metadata.Loader.GetType(resolver.Resolve(srcType.BaseType.GetTypeSig()));
				}

				if (srcType.DeclaringType != null)
				{
					mosaType.DeclaringType = metadata.Loader.GetType(resolver.Resolve(srcType.DeclaringType.GetTypeSig()));
					mosaType.Namespace = srcType.DeclaringType.Namespace;
				}

				mosaType.Interfaces.Clear();
				foreach (var iface in srcType.Interfaces)
				{
					var t = metadata.Loader.GetType(resolver.Resolve(iface.Value.GetTypeSig()));
					mosaType.Interfaces.Add(t.Name, t);
				}

				mosaType.HasOpenGenericParams = type.GetTypeSig().HasOpenGenericParameter();

				ResolveCustomAttributes(mosaType, srcType.GetUnderlyingObject<UnitDesc<TypeDef, TypeSig>>().Definition);
			}

			// Add type again to make it easier to find
			metadata.Controller.AddType(type);
		}

		private void ResolveField(MosaField field)
		{
			var resolver = new GenericArgumentResolver();

			if (field.DeclaringType.GenericArguments.Count > 0)
			{
				resolver.PushTypeGenericArguments(field.DeclaringType.GenericArguments.GetGenericArguments());
			}

			using (var mosaField = metadata.Controller.MutateField(field))
			{
				mosaField.FieldType = metadata.Loader.GetType(resolver.Resolve(field.GetFieldSig().Type));

				mosaField.HasOpenGenericParams = field.DeclaringType.HasOpenGenericParams
					|| (field.FieldType.GetTypeSig()?.HasOpenGenericParameter() == true);

				ResolveCustomAttributes(mosaField, field.GetUnderlyingObject<UnitDesc<FieldDef, FieldSig>>().Definition);
			}
		}

		private void ResolveProperty(MosaProperty property)
		{
			var resolver = new GenericArgumentResolver();

			if (property.DeclaringType.GenericArguments.Count > 0)
			{
				resolver.PushTypeGenericArguments(property.DeclaringType.GenericArguments.GetGenericArguments());
			}

			using (var mosaProperty = metadata.Controller.MutateProperty(property))
			{
				mosaProperty.PropertyType = metadata.Loader.GetType(resolver.Resolve(property.GetPropertySig().RetType));

				ResolveCustomAttributes(mosaProperty, property.GetUnderlyingObject<UnitDesc<PropertyDef, PropertySig>>().Definition);
			}
		}

		private void ResolveCustomAttributes(MosaUnit.MutatorBase unit, IHasCustomAttribute obj)
		{
			if (obj is null)
				return;

			foreach (var attr in obj.CustomAttributes)
			{
				var type = metadata.Loader.GetType(attr.AttributeType.ToTypeSig());
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


		private void ResolveSZArray(MosaType arrayType)
		{
			if (arrayType.ArrayInfo != MosaArrayInfo.Vector)
				throw new CompilerException("Type must be a SZ Array.");

			var typeSystem = arrayType.TypeSystem;
			var szHelper = typeSystem.GetTypeByName(typeSystem.CorLib, "System", "SZArrayHelper");

			using (var type = typeSystem.Controller.MutateType(arrayType))
			{
				using (var szHelperType = typeSystem.Controller.MutateType(szHelper))
				{
					//Array does not have interface implementations.

					type.Interfaces.Clear();
					// Add the methods to the mutable type
					var methods = szHelper
						.Methods
						.Where(x => x.Value.GenericArguments.Count > 0 && x.Value.GenericArguments[0] == arrayType.ElementType)
						;

					foreach (var method in methods)
					{
						// HACK: the normal Equals for methods only compares signatures which causes issues with wrong methods being removed from the list
						//(szHelperType.Methods as List<MosaMethod>).RemoveAll(x => ReferenceEquals(x, method));

						//szHelperType.Methods.Remove(method.Key);

						using (var mMethod = typeSystem.Controller.MutateMethod(method.Value))
						{
							mMethod.DeclaringType = arrayType;
						}

						type.Methods[method.Key] = method.Value;
					}

					//// Add interfaces to the type and copy properties from interfaces into type so we can expose them
					//var list = new LinkedList<MosaType>();
					//list.AddLast(typeSystem.GetTypeByName(typeSystem.CorLib, "System.Collections.Generic", "IList`1<" + arrayType.ElementType.FullName + ">"));
					//list.AddLast(typeSystem.GetTypeByName(typeSystem.CorLib, "System.Collections.Generic", "ICollection`1<" + arrayType.ElementType.FullName + ">"));
					//list.AddLast(typeSystem.GetTypeByName(typeSystem.CorLib, "System.Collections.Generic", "IEnumerable`1<" + arrayType.ElementType.FullName + ">"));
					//foreach (var iface in list)
					//{
					//	if (iface is null)
					//		continue;

					//	if (!type.Interfaces.ContainsKey(iface.Name))
					//		type.Interfaces.Add(iface.Name, iface);

					//	foreach (var property in iface.Properties)
					//	{
					//		var newProperty = typeSystem.Controller.CreateProperty(property.Value);
					//		using (var mProperty = typeSystem.Controller.MutateProperty(newProperty))
					//		{
					//			mProperty.DeclaringType = arrayType;
					//		}
					//		type.Properties.Add(newProperty.Name, newProperty);
					//	}
					//}
				}
			}
		}
	}
}
