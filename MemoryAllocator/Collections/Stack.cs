using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(StackProxy<>))]
	public unsafe struct Stack<T> where T : unmanaged
	{
		/*public struct Enumerator : System.Collections.Generic.IEnumerator<T>
		{
			private readonly Stack<T> stack;
			private readonly State* state;
			private int index;
			private T currentElement;

			internal Enumerator(Stack<T> stack, State* state)
			{
				this.stack = stack;
				this.state = state;
				index = -2;
				currentElement = default(T);
			}

			public void Dispose()
			{
				index = -1;
			}

			public bool MoveNext()
			{
				bool retval;
				if (index == -2)
				{
					// First call to enumerator.
					index = (int)stack.size - 1;
					retval = index >= 0;
					if (retval)
					{
						currentElement = stack.array[in state->allocator, index];
					}

					return retval;
				}

				if (index == -1)
				{
					// End of enumeration.
					return false;
				}

				retval = --index >= 0;
				if (retval)
				{
					currentElement = stack.array[in state->allocator, index];
				}
				else
				{
					currentElement = default(T);
				}

				return retval;
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
				index = -2;
				currentElement = default;
			}
		}*/

		private const uint DEFAULT_CAPACITY = 4u;

		private MemArray<T> array;
		private uint size;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => array.IsCreated;
		}

		public readonly uint Count => size;

		[INLINE(256)]
		public Stack(ref Allocator allocator, uint capacity, byte growFactor = 1)
		{
			this = default;
			array = new MemArray<T>(ref allocator, capacity, growFactor: growFactor);
		}

		/*public Enumerator GetEnumerator(World world)
		{
			return new Enumerator(this, world.state);
		}*/

		[INLINE(256)]
		public void* GetUnsafePtr(in Allocator allocator)
		{
			return array.GetPtr(in allocator);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			array.BurstMode(in allocator, state);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			array.Dispose(ref allocator);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			size = 0;
		}

		[INLINE(256)]
		public bool Contains<U>(in Allocator allocator, U item) where U : System.IEquatable<T>
		{
			var count = size;
			while (count-- > 0)
			{
				if (item.Equals(array[in allocator, count]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public readonly T Peek(in Allocator allocator)
		{
			E.IS_EMPTY(size);

			return array[in allocator, size - 1];
		}

		[INLINE(256)]
		public T Pop(in Allocator allocator)
		{
			E.IS_EMPTY(size);

			var item = array[in allocator, --size];
			array[in allocator, size] = default;
			return item;
		}

		[INLINE(256)]
		public void Push(ref Allocator allocator, T item)
		{
			if (size == array.Length)
			{
				array.Resize(ref allocator, array.Length == 0 ? DEFAULT_CAPACITY : 2 * array.Length);
			}

			array[in allocator, size++] = item;
		}

		/*[INLINE(256)]
		public void PushLock(ref LockSpinner spinner, ref Allocator allocator, T item)
		{
			if (size == array.Length)
			{
				spinner.Lock();
				if (size == array.Length)
				{
					array.Resize(ref allocator, array.Length == 0 ? DEFAULT_CAPACITY : 2 * array.Length);
				}

				spinner.Unlock();
			}

			var idx = JobUtils.Increment(ref size);
			array[in allocator, idx - 1u] = item;
		}

		[INLINE(256)]
		public void PushRange(ref Allocator allocator, Unity.Collections.LowLevel.Unsafe.UnsafeList<uint>* list)
		{
			var freeItems = array.Length - size;
			if (list->Length >= freeItems)
			{
				var delta = (uint)list->Length - freeItems;
				array.Resize(ref allocator, array.Length + delta, growFactor: 1);
			}

			MemoryExt.MemCopy(list->Ptr, (byte*)array.GetUnsafePtr(in allocator) + sizeof(uint) * size,
				(uint)(sizeof(uint) * list->Length));
			size += (uint)list->Length;
		}*/

		[INLINE(256)]
		public void PushNoChecks(T item, T* ptr)
		{
			*ptr = item;
			++size;
		}

		public uint GetReservedSizeInBytes()
		{
			return array.GetReservedSizeInBytes();
		}
	}
}
