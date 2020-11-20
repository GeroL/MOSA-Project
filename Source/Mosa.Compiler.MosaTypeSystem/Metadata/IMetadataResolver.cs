// Copyright (c) MOSA Project. Licensed under the New BSD License.

namespace Mosa.Compiler.MosaTypeSystem.Metadata
{
	internal interface IMetadataResolver
	{
		void EnqueueForArrayResolve(MosaType type);
		void EnqueueForResolve(MosaUnit unit);
		void Resolve();
	}
}