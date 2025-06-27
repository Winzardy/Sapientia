using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;

namespace Submodules.Sapientia.Safety
{
	internal struct DisposeSentinelAllocator
	{
		private UnsafeIndexAllocSparseSet<int> _versions;
		private AsyncValue _asyncValue;
		private int _typeId;

		public static DisposeSentinelAllocator Create(int typeId)
		{
			return new DisposeSentinelAllocator
			{
				_versions = new UnsafeIndexAllocSparseSet<int>(8),
				_asyncValue =  new AsyncValue(),
				_typeId = typeId,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DisposeSentinel AllocateSafetyHandle()
		{
			_asyncValue.SetBusy();

			var id = _versions.AllocateId();
			ref var version = ref _versions.Get(id);
			version++;

			_asyncValue.SetFree();
			return new DisposeSentinel(id, version, _typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CheckSafetyHandle(DisposeSentinel handle)
		{
			if (handle.typeId != _typeId)
				throw new ArgumentException($"Handle type id is not equal to {nameof(DisposeSentinelAllocator)} type id");
			if (!_versions.Has(handle.id))
				return false;

			var version = _versions.Get(handle.id);
			return version == handle.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseSafetyHandle(DisposeSentinel handle)
		{
			if (handle.typeId != _typeId)
				throw new ArgumentException($"Handle type id is not equal to {nameof(DisposeSentinelAllocator)} type id");
			_asyncValue.SetBusy();
			_versions.ReleaseId(handle.id);
			_asyncValue.SetFree();
		}
	}
}
