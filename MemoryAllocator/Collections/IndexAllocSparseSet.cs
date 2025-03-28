using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct IndexAllocSparseSet<T> where T: unmanaged
	{
		private Stack<int> _ids;
		private SparseSet<T> _sparseSet;

		private int _count;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.Capacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _sparseSet.IsFull;
		}

		public IndexAllocSparseSet(SafePtr<Allocator> allocator, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_ids = new Stack<int>(allocator, capacity);
			_sparseSet = new SparseSet<T>(allocator, capacity, sparseCapacity, expandStep);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return _ids.GetAllocatorPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr()
		{
			return _sparseSet.GetValuePtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(SafePtr<Allocator> allocator)
		{
			return _sparseSet.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(SafePtr<Allocator> allocator, int id)
		{
			return ref _sparseSet.Get(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(SafePtr<Allocator> allocator, int id)
		{
			return ref _sparseSet.EnsureGet(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(SafePtr<Allocator> allocator, int id)
		{
			return _sparseSet.Has(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return _sparseSet.Has(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AllocateId(SafePtr<Allocator> allocator)
		{
			if (_ids.Count <= 0)
				_ids.Push(allocator, _count + 1);

			var id = _ids.Pop(allocator);
			_count++;
			return id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseIndex(SafePtr<Allocator> allocator, int index)
		{
			var id = _sparseSet.GetIdByIndex(allocator, index);
			ReleaseId(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseId(SafePtr<Allocator> allocator, int id)
		{
			_sparseSet.RemoveSwapBack(allocator, id);
			_ids.Push(allocator, id);
			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			Dispose(GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			_ids.Dispose(allocator);
			_sparseSet.Dispose(allocator);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_ids.Clear();
			_sparseSet.Clear();
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_ids.Clear();
			_sparseSet.ClearFast();
			_count = 0;
		}
	}
}
