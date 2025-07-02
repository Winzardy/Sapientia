using System.Runtime.CompilerServices;
using Sapientia;
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
		public DisposeSentinel AllocateDisposeSentinel()
		{
			_asyncValue.SetBusy();

			var id = _versions.AllocateId();
			ref var version = ref _versions.Get(id);
			version++;

			_asyncValue.SetFree();
			return new DisposeSentinel(id, version, _typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CheckDisposeSentinel(DisposeSentinel handle)
		{
			E.ASSERT(handle.typeId == _typeId, $"Handle type id is not equal to {nameof(DisposeSentinelAllocator)} type id");

			if (!_versions.Has(handle.id))
				return false;

			var version = _versions.Get(handle.id);
			return version == handle.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseDisposeSentinel(DisposeSentinel handle)
		{
			E.ASSERT(handle.typeId == _typeId, $"Handle type id is not equal to {nameof(DisposeSentinelAllocator)} type id");

			_asyncValue.SetBusy();

			_versions.Get(handle.id)++;
			_versions.ReleaseId(handle.id, false);

			_asyncValue.SetFree();
		}
	}
}
