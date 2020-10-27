// Copyright (c) MOSA Project. Licensed under the New BSD License.

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Mosa.Compiler.Common.Exceptions;

using System;
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

		public MetadataResolver(CLRMetadata metadata)
		{
			this.metadata = metadata;

			var channel = Channel.CreateUnbounded<MosaUnit>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });
			_channelWriter = channel.Writer;
			_channelReader = channel.Reader;
			_methodResolver = new MethodResolver(metadata);
		}

		private readonly ChannelWriter<MosaUnit> _channelWriter;
		private readonly ChannelReader<MosaUnit> _channelReader;

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

					Thread.Sleep(1);
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

		public void Resolve()
		{
			foreach (var unit in metadata.Loader.LoadedUnits)
			{
				if (unit is MosaType type)
				{
					using (var mosaType = metadata.Controller.MutateType(type))
					{
						var typeDef = type.GetUnderlyingObject<UnitDesc<TypeDef, TypeSig>>().Definition;

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

						foreach (var iface in typeDef.Interfaces)
						{
							var t = metadata.Loader.GetType(iface.Interface.ToTypeSig());
							mosaType.Interfaces.Add(t.Name, t);
						}

						if (typeDef.BaseType != null)
						{
							ResolveInterfacesInBaseTypes(mosaType, type.BaseType);
						}
					}
					ResolveType(type);
				}
				else if (unit is MosaField || unit is MosaMethod || unit is MosaModule || unit is MosaProperty)
				{
					EnqueueForResolve(unit);
				}
			}

			var t1 = new Thread(new ThreadStart(ResolveThread));
			t1.Start();

			//var t2 = new Thread(new ThreadStart(ResolveThread));
			//t2.Start();

			//var t3 = new Thread(new ThreadStart(ResolveThread));
			//t3.Start();

			//var t4 = new Thread(new ThreadStart(ResolveThread));
			//t4.Start();

			while (count > 0)
			{
				Task.Delay(1000).Wait();
				Debug.WriteLine($"Resolved {resolved} {resolved - oldResolved}/s Queue: {count} Errors: {errors}");
				oldResolved = resolved;
			}

			t1.Join();


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

			while (arrayResolveQueue.Count > 0)
			{
				var type = arrayResolveQueue.Dequeue();
				ResolveSZArray(type);
			}
		}

		private void ResolveInterfacesInBaseTypes(MosaType.Mutator mosaType, MosaType baseType)
		{
			foreach (var iface in baseType.Interfaces)
			{
				if (mosaType.Interfaces.Contains(iface))
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

			return new MosaCustomAttribute.Argument(metadata.Loader.GetType(arg.Type), value);
		}

	
		private void ResolveType(MosaType type)
		{
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
					// Add the methods to the mutable type
					var methods = szHelper
						.Methods
						.Where(x => x.Value.GenericArguments.Count > 0 && x.Value.GenericArguments[0] == arrayType.ElementType)
						;

					foreach (var method in methods)
					{
						// HACK: the normal Equals for methods only compares signatures which causes issues with wrong methods being removed from the list
						//(szHelperType.Methods as List<MosaMethod>).RemoveAll(x => ReferenceEquals(x, method));

						szHelperType.Methods.Remove(method.Key);

						using (var mMethod = typeSystem.Controller.MutateMethod(method.Value))
						{
							mMethod.DeclaringType = arrayType;
						}

						type.Methods[method.Value.FullName] = method.Value;
					}

					// Add interfaces to the type and copy properties from interfaces into type so we can expose them
					var list = new LinkedList<MosaType>();
					list.AddLast(typeSystem.GetTypeByName(typeSystem.CorLib, "System.Collections.Generic", "IList`1<" + arrayType.ElementType.FullName + ">"));
					list.AddLast(typeSystem.GetTypeByName(typeSystem.CorLib, "System.Collections.Generic", "ICollection`1<" + arrayType.ElementType.FullName + ">"));
					list.AddLast(typeSystem.GetTypeByName(typeSystem.CorLib, "System.Collections.Generic", "IEnumerable`1<" + arrayType.ElementType.FullName + ">"));
					foreach (var iface in list)
					{
						if (iface is null)
							continue;

						type.Interfaces.Add(iface.Name, iface);
						foreach (var property in iface.Properties)
						{
							var newProperty = typeSystem.Controller.CreateProperty(property.Value);
							using (var mProperty = typeSystem.Controller.MutateProperty(newProperty))
							{
								mProperty.DeclaringType = arrayType;
							}
							type.Properties.Add(newProperty.Name, newProperty);
						}
					}
				}
			}
		}
	}
}
