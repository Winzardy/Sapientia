namespace Sapientia.MemoryAllocator
{
	public unsafe class MemArrayProxy<T> where T : unmanaged
	{
		private MemArray<T> _arr;

		public MemArrayProxy(MemArray<T> arr)
		{
			_arr = arr;
		}

		/*public T[] Items
		{
			get
			{
				var world = Context.world;
				var arr = new T[this.arr.Length];
				for (var i = 0; i < this.arr.Length; ++i)
				{
					arr[i] = this.arr[world.state->allocator, i];
				}

				return arr;
			}
		}*/
	}

	public unsafe class MemArrayProxy
	{
		private MemArray _arr;

		public MemArrayProxy(MemArray arr)
		{
			_arr = arr;
		}

		/*public T[] Items
		{
			get
			{
				var world = Context.world;
				var arr = new T[this.arr.Length];
				for (var i = 0; i < this.arr.Length; ++i)
				{
					arr[i] = this.arr[world.state->allocator, i];
				}

				return arr;
			}
		}*/
	}

	public unsafe class MemArrayThreadCacheLineProxy<T> where T : unmanaged
	{
		private MemArrayThreadCacheLine<T> _arr;

		public MemArrayThreadCacheLineProxy(MemArrayThreadCacheLine<T> arr)
		{
			_arr = arr;
		}

		/*public T[] items
		{
			get
			{
				var world = Context.world;
				var arr = new T[this.arr.Length];
				for (var i = 0; i < this.arr.Length; ++i)
				{
					arr[i] = this.arr[world.state->allocator, i];
				}

				return arr;
			}
		}*/
	}

	public unsafe class ListProxy<T> where T : unmanaged
	{
		private List<T> _arr;

		public ListProxy(List<T> arr)
		{
			_arr = arr;
		}

		public uint Capacity => _arr.Capacity;

		public uint Count => _arr.Count;

		public T[] Items
		{
			get
			{
				ref var allocator = ref AllocatorManager.CurrentAllocator;
				var arr = new T[_arr.Count];
				for (uint i = 0; i < _arr.Count; ++i)
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

		public uint Count => _arr.Count;

		/*public T[] items
		{
			get
			{
				var arr = new T[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(Context.world);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				e.Dispose();

				return arr;
			}
		}*/
	}

	public class StackProxy<T> where T : unmanaged
	{
		private Stack<T> _arr;

		public StackProxy(Stack<T> arr)
		{
			_arr = arr;
		}

		public uint Count => _arr.Count;

		/*public T[] items
		{
			get
			{
				var world = Context.world;
				var arr = new T[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(world);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				e.Dispose();

				return arr;
			}
		}*/
	}

	public class EquatableDictionaryProxy<TK, TV> where TK : unmanaged, System.IEquatable<TK> where TV : unmanaged
	{
		private Dictionary<TK, TV> _arr;

		public EquatableDictionaryProxy(Dictionary<TK, TV> arr)
		{
			_arr = arr;
		}

		public MemArray<uint> Buckets => _arr.buckets;
		public MemArray<Dictionary<TK, TV>.Entry> Entries => _arr.entries;
		public uint Count => _arr.count;
		public uint Version => _arr.version;
		public int FreeList => _arr.freeList;
		public uint FreeCount => _arr.freeCount;

		/*public System.Collections.Generic.KeyValuePair<K, V>[] items
		{
			get
			{
				var arr = new System.Collections.Generic.KeyValuePair<K, V>[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(Context.world);
				while (e.MoveNext())
				{
					arr[i++] = new System.Collections.Generic.KeyValuePair<K, V>(e.Current.key, e.Current.value);
				}

				return arr;
			}
		}*/
	}

	public class UIntDictionaryProxy<TV> where TV : unmanaged
	{
		private UIntDictionary<TV> _arr;

		public UIntDictionaryProxy(UIntDictionary<TV> arr)
		{
			_arr = arr;
		}

		public MemArray<uint> Buckets => _arr.buckets;
		public MemArray<UIntDictionary<TV>.Entry> Entries => _arr.entries;
		public uint Count => _arr.count;
		public uint Version => _arr.version;
		public int FreeList => _arr.freeList;
		public uint FreeCount => _arr.freeCount;

		/*public System.Collections.Generic.KeyValuePair<uint, V>[] items
		{
			get
			{
				var arr = new System.Collections.Generic.KeyValuePair<uint, V>[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(Context.world);
				while (e.MoveNext())
				{
					arr[i++] = new System.Collections.Generic.KeyValuePair<uint, V>(e.Current.key, e.Current.value);
				}

				return arr;
			}
		}*/
	}

	public class ULongDictionaryProxy<TV> where TV : unmanaged
	{
		private ULongDictionary<TV> _arr;

		public ULongDictionaryProxy(ULongDictionary<TV> arr)
		{
			_arr = arr;
		}

		public MemArray<uint> Buckets => _arr.buckets;
		public MemArray<ULongDictionary<TV>.Entry> Entries => _arr.entries;
		public uint Count => _arr.count;
		public uint Version => _arr.version;
		public int FreeList => _arr.freeList;
		public uint FreeCount => _arr.freeCount;

		/*public System.Collections.Generic.KeyValuePair<ulong, V>[] items
		{
			get
			{
				var arr = new System.Collections.Generic.KeyValuePair<ulong, V>[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(Context.world);
				while (e.MoveNext())
				{
					arr[i++] = new System.Collections.Generic.KeyValuePair<ulong, V>(e.Current.key, e.Current.value);
				}

				return arr;
			}
		}*/
	}

	/*
	public class HashSetProxy<T> where T : unmanaged {

	    private HashSet<T> arr;

	    public HashSetProxy(HashSet<T> arr) {

	        this.arr = arr;

	    }

	    public uint Count {
	        get {
	            if (StaticAllocatorProxy.allocator.isValid == false) return 0;
	            return this.arr.Count;
	        }
	    }

	    public MemArray<uint> buckets => this.arr.buckets;
	    public MemArray<HashSet<T>.Slot> slots => this.arr.slots;
	    public uint count => this.arr.count;
	    public uint version => this.arr.version;
	    public int freeList => this.arr.freeList;
	    public uint lastIndex => this.arr.lastIndex;

	    public T[] items {
	        get {
	            if (StaticAllocatorProxy.allocator.isValid == false) return null;
	            var arr = new T[this.arr.Count];
	            var i = 0;
	            var e = this.arr.GetEnumerator();
	            while (e.MoveNext() == true) {
	                arr[i++] = e.Current;
	            }
	            e.Dispose();

	            return arr;
	        }
	    }

	}
	*/

	public class UIntHashSetProxy
	{
		private UIntHashSet _arr;

		public UIntHashSetProxy(UIntHashSet arr)
		{
			_arr = arr;
		}

		public MemArray<int> Buckets => _arr.buckets;
		public MemArray<UIntHashSet.Slot> Slots => _arr.slots;
		public uint Hash => _arr.hash;
		public uint Count => (uint)_arr.count;
		public uint Version => (uint)_arr.version;
		public int FreeList => _arr.freeList;
		public uint LastIndex => (uint)_arr.lastIndex;

		/*public uint[] items
		{
			get
			{
				var arr = new uint[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(Context.world);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				return arr;
			}
		}*/
	}

	public class UIntPairHashSetProxy
	{
		private UIntPairHashSet _arr;

		public UIntPairHashSetProxy(UIntPairHashSet arr)
		{
			_arr = arr;
		}

		public MemArray<uint> Buckets => _arr.buckets;
		public MemArray<UIntPairHashSet.Slot> Slots => _arr.slots;
		public uint Hash => _arr.hash;
		public uint Count => _arr.count;
		public uint Version => _arr.version;
		public int FreeList => _arr.freeList;
		public uint LastIndex => _arr.lastIndex;

		/*public UIntPair[] items
		{
			get
			{
				var arr = new UIntPair[this.arr.Count];
				var i = 0;
				var e = this.arr.GetEnumerator(Context.world);
				while (e.MoveNext())
				{
					arr[i++] = e.Current;
				}

				return arr;
			}
		}*/
	}
}
