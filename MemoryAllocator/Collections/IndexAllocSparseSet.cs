using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct IndexAllocSparseSet<T> where T: unmanaged
	{
		private Stack<int> _indexes;
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

		public IndexAllocSparseSet(Allocator* allocator, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_indexes = new Stack<int>(allocator, capacity);
			_sparseSet = new SparseSet<T>(allocator, capacity, sparseCapacity, expandStep);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator* GetAllocatorPtr()
		{
			return _indexes.GetAllocatorPtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr()
		{
			return _sparseSet.GetValuePtr();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr(Allocator* allocator)
		{
			return _sparseSet.GetValuePtr(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(Allocator* allocator, int id)
		{
			return ref _sparseSet.Get(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(Allocator* allocator, int id)
		{
			return ref _sparseSet.EnsureGet(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(Allocator* allocator, int id)
		{
			return _sparseSet.Has(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return _sparseSet.Has(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AllocateId(Allocator* allocator)
		{
			if (_indexes.Count <= 0)
				_indexes.Push(allocator, _count + 1);

			var id = _indexes.Pop(allocator);
			_count++;
			return id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseId(Allocator* allocator, int id)
		{
			_sparseSet.RemoveSwapBack(id);
			_indexes.Push(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			Dispose(GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(Allocator* allocator)
		{
			_indexes.Dispose(allocator);
			_sparseSet.Dispose(allocator);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_indexes.Clear();
			_sparseSet.Clear();
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_indexes.Clear();
			_sparseSet.ClearFast();
			_count = 0;
		}
	}
}
