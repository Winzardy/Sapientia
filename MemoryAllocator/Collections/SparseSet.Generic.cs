using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

		public int ElementSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _innerSet.ElementSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Allocator* GetAllocatorPtr()
		{
			return _innerSet.GetAllocatorPtr();
		}

		public SparseSet(int capacity, int sparseCapacity, int expandStep = 0) : this(AllocatorManager.CurrentAllocatorPtr, capacity, sparseCapacity, expandStep) {}

		public SparseSet(AllocatorId allocatorId, int capacity, int sparseCapacity, int expandStep = 0) : this(allocatorId.GetAllocatorPtr(), capacity, sparseCapacity, expandStep) {}

		public SparseSet(Allocator* allocator, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_innerSet = new SparseSet(allocator, TSize<T>.size, capacity, sparseCapacity, expandStep);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr(Allocator* allocator)
		{
			return _innerSet.GetValuePtr<T>(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetValuePtr()
		{
			return _innerSet.GetValuePtr<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int id)
		{
			return ref _innerSet.Get<T>(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int id)
		{
			return _innerSet.Has(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(int id)
		{
			return ref _innerSet.EnsureGet<T>(id);
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
		public void Dispose(Allocator* allocator)
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
	}
}
