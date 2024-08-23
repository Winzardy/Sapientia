using Sapientia.MemoryAllocator;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	public struct SparseSet<T> where T : unmanaged
	{
		private MemArray<T> _dense;
		private MemArray<uint> _sparse;
		private Stack<uint> _freeIndexes;

		public bool IsCreated => _sparse.IsCreated;
		public uint Length => _sparse.Length;

		public SparseSet(ref Allocator allocator, uint length)
		{
			_dense = default;
			_sparse = default;
			_freeIndexes = default;
			Validate(ref allocator, length);
		}

		public void Validate(ref Allocator allocator, uint capacity)
		{
			if (_freeIndexes.IsCreated == false)
				_freeIndexes = new Stack<uint>(ref allocator, 10);
			_sparse.Resize(ref allocator, capacity);
		}

		public T ReadDense(in Allocator allocator, int sparseIndex)
		{
			return _dense[in allocator, sparseIndex];
		}

		public ref T GetDense(in Allocator allocator, int sparseIndex)
		{
			return ref _dense[in allocator, sparseIndex];
		}

		public MemArray<uint> GetSparse()
		{
			return _sparse;
		}

		public SparseSet<T> Dispose(ref Allocator allocator)
		{
			_freeIndexes.Dispose(ref allocator);
			_sparse.Dispose(ref allocator);
			_dense.Dispose(ref allocator);
			return this;
		}

		public void Set(ref Allocator allocator, int fromEntityId, int toEntityId, in T data)
		{
			for (var i = fromEntityId; i <= toEntityId; ++i)
			{
				Set(ref allocator, i, in data);
			}
		}

		public uint Set(ref Allocator allocator, int id, in T data)
		{
			ref var idx = ref _sparse[in allocator, id];
			if (idx == 0)
			{
				if (_freeIndexes.Count > 0)
				{
					idx = _freeIndexes.Pop(in allocator);
				}
				else
				{
					idx = _dense.Length + 1;
				}
			}

			_dense.Resize(ref allocator, idx + 1);
			_dense[in allocator, idx] = data;
			return idx;
		}

		public ref T Get(ref Allocator allocator, int entityId)
		{
			var idx = _sparse[in allocator, entityId];
			if (idx == 0) idx = Set(ref allocator, entityId, default);
			return ref _dense[in allocator, idx];
		}

		public T Read(ref Allocator allocator, int entityId)
		{
			var idx = _sparse[in allocator, entityId];
			if (idx == 0) return default;
			return _dense[in allocator, idx];
		}

		public void Remove(ref Allocator allocator, int entityId)
		{
			ref var idx = ref _sparse[in allocator, entityId];
			_dense[in allocator, idx] = default;
			_freeIndexes.Push(ref allocator, idx);
			idx = 0;
		}

		public void Remove(ref Allocator allocator, int entityId, int length)
		{
			for (var i = entityId; i < length; ++i)
			{
				Remove(ref allocator, i);
			}
		}

		public T Has(ref Allocator allocator, int entityId)
		{
			return Get(ref allocator, entityId);
		}

		public unsafe uint SetPtr(Allocator* allocator, int entityId, in T data)
		{
			ref var alloc = ref UnsafeExt.AsRef<Allocator>(allocator);
			return Set(ref alloc, entityId, in data);
		}

		public unsafe T ReadPtr(Allocator* allocator, int entityId)
		{
			ref var alloc = ref UnsafeExt.AsRef<Allocator>(allocator);
			var idx = _sparse[in alloc, entityId];
			if (idx == 0) return default;
			return Get(ref alloc, entityId);
		}

		public unsafe bool HasDataPtr(Allocator* allocator, int entityId)
		{
			ref var alloc = ref UnsafeExt.AsRef<Allocator>(allocator);
			var idx = _sparse[in alloc, entityId];
			if (idx == 0) return false;
			return true;
		}

		public unsafe T HasPtr(Allocator* allocator, int entityId)
		{
			return ReadPtr(allocator, entityId);
		}

		public unsafe ref T GetPtr(Allocator* allocator, int entityId)
		{
			ref var alloc = ref UnsafeExt.AsRef<Allocator>(allocator);
			return ref Get(ref alloc, entityId);
		}

		public unsafe void RemovePtr(Allocator* allocator, int entityId, int length)
		{
			ref var alloc = ref UnsafeExt.AsRef<Allocator>(allocator);
			Remove(ref alloc, entityId, length);
		}
	}
}
