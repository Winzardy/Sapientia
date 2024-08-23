namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(QueueProxy<>))]
	public unsafe struct Queue<T> where T : unmanaged
	{
		/*public struct Enumerator : System.Collections.Generic.IEnumerator<T>
		{
			private State* state;
			private Queue<T> q;
			private int index; // -1 = not started, -2 = ended/disposed
			private T currentElement;

			internal Enumerator(Queue<T> q, State* state)
			{
				this.q = q;
				index = -1;
				currentElement = default(T);
				this.state = state;
			}

			public void Dispose()
			{
				index = -2;
				currentElement = default(T);
			}

			public bool MoveNext()
			{
				if (index == -2)
				{
					return false;
				}

				index++;

				if (index == q.size)
				{
					index = -2;
					currentElement = default(T);
					return false;
				}

				currentElement = q.GetElement(in state->allocator, (uint)index);
				return true;
			}

			public T Current
			{
				get { return currentElement; }
			}

			object System.Collections.IEnumerator.Current
			{
				get { return currentElement; }
			}

			void System.Collections.IEnumerator.Reset()
			{
				index = -1;
				currentElement = default(T);
			}
		}*/

		private const uint MINIMUM_GROW = 4;
		private const uint GROW_FACTOR = 200;

		private MemArray<T> array;
		private uint head;
		private uint tail;
		private uint size;
		private uint version;
		public readonly bool isCreated => array.IsCreated;

		public readonly uint Count => size;
		public readonly uint Capacity => array.Length;

		public Queue(ref Allocator allocator, uint capacity)
		{
			this = default;
			array = new MemArray<T>(ref allocator, capacity);
		}

		public void Dispose(ref Allocator allocator)
		{
			array.Dispose(ref allocator);
			this = default;
		}

		/*public readonly Enumerator GetEnumerator(World world)
		{
			return new Enumerator(this, world.state);
		}

		public readonly Enumerator GetEnumerator(State* state)
		{
			return new Enumerator(this, state);
		}*/

		public void Clear()
		{
			head = 0;
			tail = 0;
			size = 0;
			version++;
		}

		public void Enqueue(ref Allocator allocator, T item)
		{
			if (size == array.Length)
			{
				var newCapacity = (uint)((long)array.Length * (long)GROW_FACTOR / 100L);
				if (newCapacity < array.Length + MINIMUM_GROW)
				{
					newCapacity = array.Length + MINIMUM_GROW;
				}

				SetCapacity(ref allocator, newCapacity);
			}

			array[in allocator, tail] = item;
			tail = (tail + 1) % array.Length;
			size++;
			version++;
		}

		public T Dequeue(ref Allocator allocator)
		{
			E.IS_EMPTY(size);

			var removed = array[in allocator, head];
			array[in allocator, head] = default(T);
			head = (head + 1) % array.Length;
			size--;
			version++;
			return removed;
		}

		public T Peek(in Allocator allocator)
		{
			E.IS_EMPTY(size);

			return array[in allocator, head];
		}

		public bool Contains<U>(in Allocator allocator, U item) where U : System.IEquatable<T>
		{
			var index = head;
			var count = size;

			while (count-- > 0)
			{
				if (item.Equals(array[in allocator, index]))
				{
					return true;
				}

				index = (index + 1) % array.Length;
			}

			return false;
		}

		private T GetElement(in Allocator allocator, uint i)
		{
			return array[in allocator, (head + i) % array.Length];
		}

		private void SetCapacity(ref Allocator allocator, uint capacity)
		{
			array.Resize(ref allocator, capacity);
			head = 0;
			tail = size == capacity ? 0 : size;
			version++;
		}
	}
}
