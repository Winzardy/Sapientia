using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator
{
	using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(ULongDictionaryProxy<>))]
	public unsafe struct ULongDictionary<TValue> where TValue : unmanaged
	{
		/*public struct Enumerator
		{
			private uint count;
			private readonly Entry* entries;
			private uint index;

			internal Enumerator(in ULongDictionary<TValue> dictionary, State* state)
			{
				entries = (Entry*)dictionary.entries.GetUnsafePtrCached(in state->allocator);
				count = dictionary.count;
				index = 0u;
			}

			public bool MoveNext()
			{
				while (index < count)
				{
					ref var local = ref entries[index++];
					if (local.hashCode >= 0)
					{
						return true;
					}
				}

				index = count + 1u;
				return false;
			}

			public ref Entry Current => ref *(entries + index - 1u);
		}*/

		public struct Entry
		{
			public int hashCode; // Lower 31 bits of hash code, -1 if unused
			public int next; // Index of next entry, -1 if last
			public ulong key; // Key of entry
			public TValue value; // Value of entry
		}

		internal MemArray<uint> buckets;
		internal MemArray<Entry> entries;
		internal uint count;
		internal uint version;
		internal int freeList;
		internal uint freeCount;

		public bool isCreated
		{
			[INLINE(256)] get => buckets.IsCreated;
		}

		public readonly uint Count
		{
			[INLINE(256)] get => count - freeCount;
		}

		[INLINE(256)]
		public ULongDictionary(ref Allocator allocator, uint capacity)
		{
			this = default;
			Initialize(ref allocator, capacity);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			buckets.BurstMode(in allocator, state);
			entries.BurstMode(in allocator, state);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			buckets.Dispose(ref allocator);
			entries.Dispose(ref allocator);
			this = default;
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in ULongDictionary<TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
				return;

			Dispose(ref allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in ULongDictionary<TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
				return;
			if (!GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
				return;
			if (GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
			{
				Dispose(ref allocator);
				return;
			}

			if (!GetMemPtr().IsValid())
				this = new ULongDictionary<TValue>(ref allocator, other.Count);

			MemArrayExt.CopyExact(ref allocator, other.buckets, ref buckets);
			MemArrayExt.CopyExact(ref allocator, other.entries, ref entries);
			count = other.count;
			version = other.version;
			freeCount = other.freeCount;
			freeList = other.freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public readonly ref TValue this[in Allocator allocator, ulong key]
		{
			[INLINE(256)]
			get
			{
				var entry = FindEntry(in allocator, key);
				if (entry >= 0)
				{
					return ref entries[in allocator, entry].value;
				}

				throw new System.Collections.Generic.KeyNotFoundException();
			}
		}

		[INLINE(256)]
		public ref TValue GetValue(ref Allocator allocator, ulong key)
		{
			var entry = FindEntry(in allocator, key);
			if (entry >= 0)
			{
				return ref entries[in allocator, entry].value;
			}

			return ref Insert(ref allocator, key, default);
		}

		[INLINE(256)]
		public ref TValue GetValue(ref Allocator allocator, ulong key, out bool exist)
		{
			var entry = FindEntry(in allocator, key);
			if (entry >= 0)
			{
				exist = true;
				return ref entries[in allocator, entry].value;
			}

			exist = false;
			return ref Insert(ref allocator, key, default);
		}

		[INLINE(256)]
		public TValue GetValueAndRemove(in Allocator allocator, ulong key)
		{

			Remove(in allocator, key, out var value);
			return value;
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(ref Allocator allocator, ulong key, TValue value)
		{
			TryInsert(ref allocator, key, value, InsertionBehavior.ThrowOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[INLINE(256)]
		public void Clear(in Allocator allocator)
		{
			var newCount = count;
			if (newCount > 0)
			{
				buckets.Clear(allocator);
				count = 0;
				freeList = -1;
				freeCount = 0;
				entries.Clear(allocator, 0, newCount);
			}

			++version;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[INLINE(256)]
		public bool ContainsKey(in Allocator allocator, ulong key)
		{
			return FindEntry(in allocator, key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[INLINE(256)]
		public readonly bool ContainsValue(in Allocator allocator, TValue value)
		{
			for (var index = 0; index < count; ++index)
			{
				if (entries[in allocator, index].hashCode >= 0 &&
				    System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(
					    entries[in allocator, index].value, value))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		private int FindEntry(in Allocator allocator, ulong key)
		{
			var index = -1;
			var num1 = 0;
			if (buckets.Length > 0u)
			{
				var num2 = key.GetHashCode() & int.MaxValue;
				index = (int)buckets[in allocator, (uint)(num2 % buckets.Length)] - 1;
				var entriesPtr = (Entry*)entries.GetPtr(in allocator);
				while ((uint)index < entries.Length &&
				       (entriesPtr[index].hashCode != num2 || !entriesPtr[index].key.Equals(key)))
				{
					index = entriesPtr[index].next;
					if (num1 >= entries.Length)
					{
						E.OUT_OF_RANGE();
					}

					++num1;
				}
			}

			return index;
		}

		[INLINE(256)]
		private uint Initialize(ref Allocator allocator, uint capacity)
		{
			var prime = HashHelpers.GetPrime(capacity);
			freeList = -1;
			buckets = new MemArray<uint>(ref allocator, prime);
			entries = new MemArray<Entry>(ref allocator, prime);
			return prime;
		}

		[INLINE(256)]
		private bool TryInsert(ref Allocator allocator, ulong key, TValue value, InsertionBehavior behavior)
		{
			++version;
			if (buckets.IsCreated == false)
			{
				Initialize(ref allocator, 0);
			}

			var entriesPtr = (Entry*)entries.GetPtr(in allocator);
			var num1 = key.GetHashCode() & int.MaxValue;
			var num2 = 0u;
			ref var local1 = ref buckets[in allocator, (uint)(num1 % buckets.Length)];
			var index1 = (int)local1 - 1;
			{
				while ((uint)index1 < entries.Length)
				{
					if (entriesPtr[index1].hashCode == num1 &&
					    entriesPtr[index1].key.Equals(key))
					{
						switch (behavior)
						{
							case InsertionBehavior.OverwriteExisting:
								entriesPtr[index1].value = value;
								return true;

							case InsertionBehavior.ThrowOnExisting:
								E.ADDING_DUPLICATE();
								break;
						}

						return false;
					}

					index1 = entriesPtr[index1].next;
					if (num2 >= entries.Length)
					{
						E.OUT_OF_RANGE();
					}

					++num2;
				}
			}
			var flag1 = false;
			var flag2 = false;
			uint index2;
			if (freeCount > 0)
			{
				index2 = (uint)freeList;
				flag2 = true;
				--freeCount;
			}
			else
			{
				var count = this.count;
				if (count == entries.Length)
				{
					Resize(ref allocator);
					flag1 = true;
				}

				index2 = count;
				this.count = count + 1;
				entriesPtr = (Entry*)entries.GetPtr(in allocator);
			}

			ref var local2 = ref (flag1 ? ref buckets[in allocator, (uint)(num1 % buckets.Length)] : ref local1);
			ref var local3 = ref entriesPtr[index2];
			if (flag2)
			{
				freeList = local3.next;
			}

			local3.hashCode = num1;
			local3.next = (int)local2 - 1;
			local3.key = key;
			local3.value = value;
			local2 = index2 + 1;
			return true;
		}

		[INLINE(256)]
		private ref TValue Insert(ref Allocator allocator, ulong key, TValue value)
		{
			++version;
			if (buckets.IsCreated == false)
			{
				Initialize(ref allocator, 0);
			}

			var entriesPtr = (Entry*)entries.GetPtr(in allocator);
			var num1 = key.GetHashCode() & int.MaxValue;
			ref var local1 = ref buckets[in allocator, (uint)(num1 % buckets.Length)];
			var flag1 = false;
			var flag2 = false;
			uint index2;
			if (freeCount > 0)
			{
				index2 = (uint)freeList;
				flag2 = true;
				--freeCount;
			}
			else
			{
				var oldCount = count;
				if (oldCount == entries.Length)
				{
					Resize(ref allocator);
					flag1 = true;
				}

				index2 = oldCount;
				count = oldCount + 1;
				entriesPtr = (Entry*)entries.GetPtr(in allocator);
			}

			ref var local2 = ref (flag1 ? ref buckets[in allocator, (uint)(num1 % buckets.Length)] : ref local1);
			ref var local3 = ref entriesPtr[index2];
			if (flag2)
			{
				freeList = local3.next;
			}

			local3.hashCode = num1;
			local3.next = (int)local2 - 1;
			local3.key = key;
			local3.value = value;
			local2 = index2 + 1;
			return ref local3.value;
		}

		[INLINE(256)]
		private void Resize(ref Allocator allocator)
		{
			Resize(ref allocator, HashHelpers.ExpandPrime(count));
		}

		[INLINE(256)]
		private void Resize(ref Allocator allocator, uint newSize)
		{
			var numArray = new MemArray<uint>(ref allocator, newSize);
			var entryArray = new MemArray<Entry>(ref allocator, newSize);

			MemArrayExt.CopyNoChecks(ref allocator, entries, 0, ref entryArray, 0, count);
			for (var index1 = 0u; index1 < count; ++index1)
			{
				if (entryArray[in allocator, index1].hashCode >= 0)
				{
					var index2 = (uint)(entryArray[in allocator, index1].hashCode % newSize);
					entryArray[in allocator, index1].next = (int)numArray[in allocator, index2] - 1;
					numArray[in allocator, index2] = index1 + 1u;
				}
			}

			if (buckets.IsCreated)
			{
				buckets.Dispose(ref allocator);
			}

			if (entries.IsCreated)
			{
				entries.Dispose(ref allocator);
			}

			buckets = numArray;
			entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		[INLINE(256)]
		public bool Remove(in Allocator allocator, ulong key)
		{

			if (buckets.Length > 0u)
			{
				var num = key.GetHashCode() & int.MaxValue;
				var index1 = (int)(num % buckets.Length);
				var index2 = -1;
				// ISSUE: variable of a reference type
				var next = 0;
				for (var index3 = (int)buckets[in allocator, index1] - 1; index3 >= 0; index3 = next)
				{
					ref var local = ref entries[in allocator, index3];
					next = local.next;
					if (local.hashCode == num)
					{
						if ((local.key.Equals(key) ? 1 : 0) != 0)
						{
							if (index2 < 0)
							{
								buckets[in allocator, index1] = (uint)(local.next + 1);
							}
							else
							{
								entries[in allocator, index2].next = local.next;
							}

							local.hashCode = -1;
							local.next = freeList;

							freeList = index3;
							++freeCount;
							++version;
							return true;
						}
					}

					index2 = index3;
				}
			}

			return false;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public bool Remove(in Allocator allocator, ulong key, out TValue value)
		{

			if (buckets.Length > 0u)
			{
				var num = key.GetHashCode() & int.MaxValue;
				var index1 = (int)(num % buckets.Length);
				var index2 = -1;
				// ISSUE: variable of a reference type
				var next = 0;
				for (var index3 = (int)buckets[in allocator, index1] - 1; index3 >= 0; index3 = next)
				{
					ref var local = ref entries[in allocator, index3];
					next = local.next;
					if (local.hashCode == num)
					{
						if ((local.key.Equals(key) ? 1 : 0) != 0)
						{
							if (index2 < 0)
							{
								buckets[in allocator, index1] = (uint)(local.next + 1);
							}
							else
							{
								entries[in allocator, index2].next = local.next;
							}

							value = local.value;
							local.hashCode = -1;
							local.next = freeList;

							freeList = index3;
							++freeCount;
							++version;
							return true;
						}
					}

					index2 = index3;
				}
			}

			value = default;
			return false;
		}

		/// <summary>To be added.</summary>
		/// <param name="allocator"></param>
		/// <param name="key">To be added.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public readonly bool TryGetValue(in Allocator allocator, ulong key, out TValue value)
		{

			var entry = FindEntry(in allocator, key);
			if (entry >= 0)
			{
				value = entries[in allocator, entry].value;
				return true;
			}

			value = default;
			return false;
		}

		[INLINE(256)]
		public bool TryAdd(ref Allocator allocator, ulong key, TValue value)
		{

			return TryInsert(ref allocator, key, value, InsertionBehavior.None);
		}

		[INLINE(256)]
		public uint EnsureCapacity(ref Allocator allocator, uint capacity)
		{

			var num = entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (buckets.IsCreated == false)
			{
				return Initialize(ref allocator, capacity);
			}

			var prime = HashHelpers.GetPrime(capacity);
			Resize(ref allocator, prime);
			return prime;
		}
	}
}
