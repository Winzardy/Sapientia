using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SparseSet<T> : IListEnumerable<T> where T : unmanaged
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

		public SparseSet(int capacity, int sparseCapacity, int expandStep = 0) : this(WorldManager.CurrentWorld, capacity, sparseCapacity, expandStep) {}

		public SparseSet(WorldId worldId, int capacity, int sparseCapacity, int expandStep = 0) : this(worldId.GetWorld(), capacity, sparseCapacity, expandStep) {}

		public SparseSet(World world, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_innerSet = new SparseSet(world, TSize<T>.size, capacity, sparseCapacity, expandStep);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(World world)
		{
			return _innerSet.GetValuePtr<T>(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(World world, int id)
		{
			return ref _innerSet.Get<T>(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIdByIndex(World world, int denseId)
		{
			return _innerSet.GetIdByIndex(world, denseId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(World world, int id)
		{
			return _innerSet.Has(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(World world, int id)
		{
			return ref _innerSet.EnsureGet<T>(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBack(World world, int id)
		{
			_innerSet.RemoveSwapBack(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world)
		{
			_innerSet.Dispose(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(World world)
		{
			_innerSet.Clear(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearFast()
		{
			_innerSet.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(World world)
		{
			return new (new (GetValuePtr(world), Count));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(World world)
		{
			return new (new (GetValuePtr(world), 0, Count));
		}
	}
}
