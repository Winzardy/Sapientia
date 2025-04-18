using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SparseSet<T> : IListEnumerable<T> where T : unmanaged
	{
		private SparseSet _innerSet;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.Count;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.Capacity;
		}

		public int FreeIndexesCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.FreeIndexesCapacity;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.IsFull;
		}

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.IsCreated;
		}

		public int ExpandStep
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.expandStep;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return _innerSet.GetAllocatorPtr();
		}

		public SparseSet(int capacity, int sparseCapacity, int expandStep = 0) : this(AllocatorManager.CurrentAllocatorPtr, capacity, sparseCapacity, expandStep) {}

		public SparseSet(AllocatorId allocatorId, int capacity, int sparseCapacity, int expandStep = 0) : this(allocatorId.GetAllocatorPtr(), capacity, sparseCapacity, expandStep) {}

		public SparseSet(SafePtr<Allocator> allocator, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_innerSet = new SparseSet(allocator, TSize<T>.size, capacity, sparseCapacity, expandStep);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(SafePtr<Allocator> allocator)
		{
			return _innerSet.GetValuePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr()
		{
			return _innerSet.GetValuePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(SafePtr<Allocator> allocator, int id)
		{
			return ref _innerSet.Get<T>(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int id)
		{
			return ref _innerSet.Get<T>(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByIndex(SafePtr<Allocator> allocator, int denseId)
		{
			return _innerSet.GetIdByIndex(allocator, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(SafePtr<Allocator> allocator, int id)
		{
			return _innerSet.Has(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return _innerSet.Has(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(SafePtr<Allocator> allocator, int id)
		{
			return ref _innerSet.EnsureGet<T>(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(int id)
		{
			return ref _innerSet.EnsureGet<T>(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBack(SafePtr<Allocator> allocator, int id)
		{
			_innerSet.RemoveSwapBack(allocator, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBack(int id)
		{
			_innerSet.RemoveSwapBack(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_innerSet.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			_innerSet.Dispose(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_innerSet.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_innerSet.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetValuePtr(allocator), 0, Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable()
		{
			return new (new (GetValuePtr(), 0, Count));
		}
	}
}
