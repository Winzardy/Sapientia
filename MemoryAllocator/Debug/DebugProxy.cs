namespace Sapientia.MemoryAllocator
{
	public unsafe class MemArrayProxy<T> where T : unmanaged
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
				var allocator = _arr.GetAllocatorPtr();
				var arr = new T[_arr.Length];
				for (var i = 0; i < _arr.Length; ++i)
				{
					arr[i] = _arr[allocator, i];
				}

				return arr;
			}
		}
	}

	/*public unsafe class MemArrayProxy
	{
		private MemArray _arr;

		public MemArrayProxy(MemArray arr)
		{
			_arr = arr;
		}

		public object[] Items
		{
			get
			{
				var allocator = _arr.GetAllocatorPtr();
				var arr = new object[_arr.Length];
				for (var i = 0; i < _arr.Length; ++i)
				{
					arr[i] = _arr.GetValue(allocator, i);
				}

				return arr;
			}
		}
	}*/

	public unsafe class ListProxy<T> where T : unmanaged
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
				var allocator = _arr.GetAllocatorPtr();
				var arr = new T[_arr.Count];
				for (int i = 0; i < _arr.Count; ++i)
				{
					arr[i] = _arr[allocator, i];
				}

				return arr;
			}
		}
	}

	public class QueueProxy<T> where T : unmanaged
	{
		private Queue<T> _arr;

		public QueueProxy(Queue<T> arr)
		{
			_arr = arr;
		}

		public int Count => _arr.Count;

		public T[] Items
		{
			get
			{
				var arr = new T[_arr.Count];
				var i = 0;
				var e = _arr.GetEnumerator();
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
				var allocator = _arr.GetAllocatorPtr();
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
		public int LastIndex => _arr.lastIndex;
		public int FreeList => _arr.freeList;
		public int FreeCount => _arr.freeCount;

		public System.Collections.Generic.KeyValuePair<TK, TV>[] Items
		{
			get
			{
				var arr = new System.Collections.Generic.KeyValuePair<TK, TV>[_arr.Count];
				var i = 0;
				var e = _arr.GetEnumerator();
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
				var e = _arr.GetEnumerator();
				while (e.MoveNext())
				{
					arr[i++] = e.Current.value;
				}

				e.Dispose();

				return arr;
			}
		}
	}
}
