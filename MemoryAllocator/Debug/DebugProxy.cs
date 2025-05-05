#if UNITY_EDITOR

namespace Sapientia.MemoryAllocator
{
	public class MemArrayProxy<T> where T : unmanaged
	{
		private MemArray<T> _arr;

		public MemArrayProxy(MemArray<T> arr)
		{
			_arr = arr;
		}

		public T[] Items
		{
			get
			{
				var world = _arr.GetWorld();
				var arr = new T[_arr.Length];
				for (var i = 0; i < _arr.Length; ++i)
				{
					arr[i] = _arr[world, i];
				}

				return arr;
			}
		}
	}

	public class ListProxy<T> where T : unmanaged
	{
		private List<T> _arr;

		public ListProxy(List<T> arr)
		{
			_arr = arr;
		}

		public int Capacity => _arr.Capacity;

		public int Count => _arr.Count;

		public T[] Items
		{
			get
			{
				var world = _arr.GetWorld();
				var arr = new T[_arr.Count];
				for (int i = 0; i < _arr.Count; ++i)
				{
					arr[i] = _arr[world, i];
				}

				return arr;
			}
		}
	}

	public class QueueProxy<T> where T : unmanaged
	{
		private Queue<T> _queue;

		public QueueProxy(Queue<T> queue)
		{
			_queue = queue;
		}

		public int Count => _queue.Count;

		public T[] Items
		{
			get
			{
				var arr = new T[_queue.Count];
				var i = 0;
				var world = _queue.GetWorld();
				var e = _queue.GetEnumerator(world);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				e.Dispose();

				return arr;
			}
		}
	}

	public unsafe class StackProxy<T> where T : unmanaged
	{
		private Stack<T> _arr;

		public StackProxy(Stack<T> arr)
		{
			_arr = arr;
		}

		public int Count => _arr.Count;

		public T[] Items
		{
			get
			{
				var allocator = _arr.GetWorld();
				var arr = new T[_arr.Count];
				var i = 0;
				var e = _arr.GetEnumerator(allocator);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				e.Dispose();

				return arr;
			}
		}
	}

	public class EquatableDictionaryProxy<TK, TV> where TK : unmanaged, System.IEquatable<TK> where TV : unmanaged
	{
		private Dictionary<TK, TV> _arr;

		public EquatableDictionaryProxy(Dictionary<TK, TV> arr)
		{
			_arr = arr;
		}

		public MemArray<int> Buckets => _arr.buckets;
		public MemArray<Dictionary<TK, TV>.Entry> Entries => _arr.entries;
		public int Count => _arr.count;
		public int FreeList => _arr.freeList;
		public int FreeCount => _arr.freeCount;

		public System.Collections.Generic.KeyValuePair<TK, TV>[] Items
		{
			get
			{
				var arr = new System.Collections.Generic.KeyValuePair<TK, TV>[_arr.Count];
				var i = 0;
				var world = _arr.GetWorld();
				var e = _arr.GetEnumerator(world);
				while (e.MoveNext())
				{
					arr[i++] = new System.Collections.Generic.KeyValuePair<TK, TV>(e.Current.key, e.Current.value);
				}

				return arr;
			}
		}
	}

	public class HashSetProxy<T> where T : unmanaged, System.IEquatable<T>
	{
		private HashSet<T> _arr;

		public HashSetProxy(HashSet<T> arr)
		{
			_arr = arr;
		}

		public MemArray<int> Buckets => _arr.buckets;
		public MemArray<HashSet<T>.Slot> Slots => _arr.slots;
		public int Count => _arr.count;
		public int FreeList => _arr.freeList;
		public int LastIndex => _arr.lastIndex;

		public T[] Items
		{
			get
			{
				var arr = new T[_arr.Count];
				var i = 0;
				var world = _arr.GetWorld();
				var e = _arr.GetEnumerator(world);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				e.Dispose();

				return arr;
			}
		}
	}
}

#endif
