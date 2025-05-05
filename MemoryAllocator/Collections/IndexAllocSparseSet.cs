using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

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

		public IndexAllocSparseSet(World world, int capacity, int sparseCapacity, int expandStep = 0)
		{
			_ids = new Stack<int>(world, capacity);
			_sparseSet = new SparseSet<T>(world, capacity, sparseCapacity, expandStep);
			_count = 0;
		}

#if UNITY_EDITOR
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal World GetWorld()
		{
			return _ids.GetWorld();
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(World world)
		{
			return _sparseSet.GetValuePtr(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(World world, int id)
		{
			return ref _sparseSet.Get(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T EnsureGet(World world, int id)
		{
			return ref _sparseSet.EnsureGet(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(World world, int id)
		{
			return _sparseSet.Has(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AllocateId(World world)
		{
			if (_ids.Count <= 0)
				_ids.Push(world, _count + 1);

			var id = _ids.Pop(world);
			_count++;
			return id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseIndex(World world, int index)
		{
			var id = _sparseSet.GetIdByIndex(world, index);
			ReleaseId(world, id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReleaseId(World world, int id)
		{
			_sparseSet.RemoveSwapBack(world, id);
			_ids.Push(world, id);
			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world)
		{
			_ids.Dispose(world);
			_sparseSet.Dispose(world);
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(World world)
		{
			_ids.Clear();
			_sparseSet.Clear(world);
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
