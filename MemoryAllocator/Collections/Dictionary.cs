using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator
{
	using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

	[DebuggerTypeProxy(typeof(EquatableDictionaryProxy<,>))]
	public unsafe struct Dictionary<TKey, TValue> : IDictionaryEnumerable<TKey, TValue>
		where TKey : unmanaged, IEquatable<TKey>
		where TValue : unmanaged
	{
		public struct Entry
		{
			public int hashCode; // Lower 31 bits of hash code, -1 if unused
			public int next; // Index of next entry, -1 if last
			public TKey key; // Key of entry
			public TValue value; // Value of entry
		}

		internal MemArray<int> buckets;
		internal MemArray<Entry> entries;
		internal int lastIndex;
		internal int freeList;
		internal int freeCount;

		public bool IsCreated
		{
			[INLINE(256)] get => buckets.IsCreated;
		}

		public readonly int Count
		{
			[INLINE(256)] get => lastIndex - freeCount;
		}

		public readonly int LastIndex
		{
			[INLINE(256)] get => lastIndex;
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return buckets.GetAllocatorPtr();
		}

		[INLINE(256)]
		public Dictionary(Allocator* allocator, int capacity)
		{
			this = default;
			Initialize(allocator, capacity);
		}

		[INLINE(256)]
		public Entry* GetEntryPtr()
		{
			return entries.GetValuePtr();
		}

		[INLINE(256)]
		public Entry* GetEntryPtr(Allocator* allocator)
		{
			return entries.GetValuePtr(allocator);
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			buckets.Dispose(allocator);
			entries.Dispose(allocator);
			this = default;
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			return buckets.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void ReplaceWith(Allocator* allocator, in Dictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr()) return;

			Dispose(allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* allocator, in Dictionary<TKey, TValue> other)
		{
			if (GetMemPtr() == other.GetMemPtr())
				return;
			if (!GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
				return;
			if (GetMemPtr().IsValid() && !other.GetMemPtr().IsValid())
			{
				Dispose(allocator);
				return;
			}

			if (GetMemPtr().IsValid() == false)
				this = new Dictionary<TKey, TValue>(allocator, other.Count);

			MemArrayExt.CopyExact(allocator, other.buckets, ref buckets);
			MemArrayExt.CopyExact(allocator, other.entries, ref entries);
			lastIndex = other.lastIndex;
			freeCount = other.freeCount;
			freeList = other.freeList;
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public readonly ref TValue this[Allocator* allocator, in TKey key]
		{
			[INLINE(256)]
			get
			{
				var entry = FindEntry(allocator, key);
				if (entry >= 0)
				{
					return ref entries[allocator, entry].value;
				}

				throw new System.Collections.Generic.KeyNotFoundException();
			}
		}

		/// <summary><para>Gets or sets the value associated with the specified key.</para></summary>
		/// <param name="key">The key whose value is to be gotten or set.</param>
		public ref TValue this[in TKey key]
		{
			[INLINE(256)]
			get
			{
				var allocator = GetAllocatorPtr();
				var entry = FindEntry(allocator, key);
				if (entry >= 0)
				{
					return ref entries[allocator, entry].value;
				}

				throw new System.Collections.Generic.KeyNotFoundException();
			}
		}

		[INLINE(256)]
		public ref TValue GetValue(TKey key)
		{
			var allocator = GetAllocatorPtr();

			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				return ref entries[allocator, entry].value;
			}

			TryInsert(allocator, key, default, InsertionBehavior.overwriteExisting);
			return ref entries[allocator, FindEntry(allocator, key)].value;
		}

		[INLINE(256)]
		public ref TValue GetValue(Allocator* allocator, TKey key)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				return ref entries[allocator, entry].value;
			}

			TryInsert(allocator, key, default, InsertionBehavior.overwriteExisting);
			return ref entries[allocator, FindEntry(allocator, key)].value;
		}

		[INLINE(256)]
		public ref TValue GetValue(Allocator* allocator, TKey key, out bool success)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				success = true;
				return ref entries[allocator, entry].value;
			}

			success = false;
			return ref TDefaultValue<TValue>.value;
		}

		[INLINE(256)]
		public bool TryGetValue(Allocator* allocator, TKey key, out TValue value)
		{
			var entry = FindEntry(allocator, key);
			if (entry >= 0)
			{
				value = entries[allocator, entry].value;
				return true;
			}

			value = TDefaultValue<TValue>.value;
			return false;
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(TKey key, TValue value)
		{
			TryInsert(GetAllocatorPtr(), key, value, InsertionBehavior.throwOnExisting);
		}

		/// <summary><para>Adds an element with the specified key and value to the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to add to the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public void Add(Allocator* allocator, TKey key, TValue value)
		{
			TryInsert(allocator, key, value, InsertionBehavior.throwOnExisting);
		}

		/// <summary><para>Removes all elements from the dictionary.</para></summary>
		[INLINE(256)]
		public void Clear(Allocator* allocator)
		{
			var clearCount = lastIndex;
			if (clearCount > 0)
			{
				buckets.Clear(allocator);
				lastIndex = 0;
				freeList = -1;
				freeCount = 0;
				entries.Clear(allocator, 0, clearCount);
			}
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific key.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key to locate in the dictionary.</param>
		[INLINE(256)]
		public readonly bool ContainsKey(Allocator* allocator, TKey key)
		{
			return FindEntry(allocator, key) >= 0;
		}

		/// <summary><para>Determines whether the dictionary contains an element with a specific value.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="value">The value to locate in the dictionary.</param>
		[INLINE(256)]
		public readonly bool ContainsValue(Allocator* allocator, TValue value)
		{
			for (var index = 0; index < lastIndex; ++index)
			{
				if (entries[allocator, index].hashCode >= 0 && System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(entries[allocator, index].value, value))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		private readonly int FindEntry(Allocator* allocator, in TKey key)
		{
			var index = -1;
			var num1 = 0;
			if (buckets.IsCreated)
			{
				var num2 = key.GetHashCode() & int.MaxValue;
				index = buckets[allocator, (num2 % buckets.Length)] - 1;
				while (index < entries.Length && (entries[allocator, index].hashCode != num2 || !entries[allocator, index].key.Equals(key)))
				{
					index = entries[allocator, index].next;
					Debug.Assert(num1 < entries.Length);

					++num1;
				}
			}

			return index;
		}

		[INLINE(256)]
		private int Initialize(Allocator* allocator, int capacity)
		{
			var prime = HashHelpers.GetPrime(capacity);
			freeList = -1;
			buckets = new MemArray<int>(allocator, prime);
			entries = new MemArray<Entry>(allocator, prime);
			return prime;
		}

		[INLINE(256)]
		private bool TryInsert(Allocator* allocator, TKey key, TValue value, InsertionBehavior behavior)
		{
			if (!buckets.IsCreated)
			{
				Initialize(allocator, 0);
			}

			var num1 = key.GetHashCode() & int.MaxValue;
			var num2 = 0u;
			ref var local1 = ref buckets[allocator, (num1 % buckets.Length)];
			var index1 = local1 - 1;
			if (index1 >= 0)
			{
				while (index1 < entries.Length)
				{
					if (entries[allocator, index1].hashCode == num1 && entries[allocator, index1].key.Equals(key))
					{
						switch (behavior)
						{
							case InsertionBehavior.overwriteExisting:
								entries[allocator, index1].value = value;
								return true;

							case InsertionBehavior.throwOnExisting:
								E.ADDING_DUPLICATE();
								break;
						}

						return false;
					}

					index1 = entries[allocator, index1].next;
					if (num2 >= entries.Length)
					{
						E.OUT_OF_RANGE();
					}

					++num2;
				}
			}
			var flag1 = false;
			var flag2 = false;
			int index2;
			if (freeCount > 0)
			{
				index2 = freeList;
				flag2 = true;
				--freeCount;
			}
			else
			{
				var count = this.lastIndex;
				if (count == entries.Length)
				{
					Resize(allocator);
					flag1 = true;
				}

				index2 = count;
				this.lastIndex = count + 1;
			}

			ref var local2 = ref (flag1 ? ref buckets[allocator, (num1 % buckets.Length)] : ref local1);
			ref var local3 = ref entries[allocator, index2];
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
		private void Resize(Allocator* allocator)
		{
			Resize(allocator, HashHelpers.ExpandPrime(lastIndex));
		}

		[INLINE(256)]
		private void Resize(Allocator* allocator, int newSize)
		{
			var numArray = new MemArray<int>(allocator, newSize);
			var entryArray = new MemArray<Entry>(allocator, newSize);

			MemArrayExt.CopyNoChecks(allocator, entries, 0, ref entryArray, 0, lastIndex);
			for (var index1 = 0; index1 < lastIndex; ++index1)
			{
				if (entryArray[allocator, index1].hashCode >= 0)
				{
					var index2 = (entryArray[allocator, index1].hashCode % newSize);
					entryArray[allocator, index1].next = numArray[allocator, index2] - 1;
					numArray[allocator, index2] = index1 + 1;
				}
			}

			if (buckets.IsCreated)
			{
				buckets.Dispose(allocator);
			}

			if (entries.IsCreated)
			{
				entries.Dispose(allocator);
			}

			buckets = numArray;
			entries = entryArray;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		[INLINE(256)]
		public bool Remove(TKey key)
		{
			return Remove(GetAllocatorPtr(), key);
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		[INLINE(256)]
		public bool Remove(Allocator* allocator, TKey key)
		{
			if (!buckets.IsCreated)
				return false;

			var hashCode = key.GetHashCode() & int.MaxValue;
			var buckedIndex = (hashCode % buckets.Length);
			var previousIndex = -1;

			var nextIndex = buckets[allocator, buckedIndex] - 1;
			while (nextIndex >= 0)
			{
				ref var local = ref entries[allocator, nextIndex];
				nextIndex = local.next;

				if (local.hashCode == hashCode && local.key.Equals(key))
				{
					if (previousIndex < 0)
					{
						buckets[allocator, buckedIndex] = (local.next + 1);
					}
					else
					{
						entries[allocator, previousIndex].next = local.next;
					}

					local.hashCode = -1;
					local.next = freeList;

					freeList = nextIndex;
					freeCount++;

					return true;
				}

				previousIndex = nextIndex;
			}

			return false;
		}

		/// <summary><para>Removes the element with the specified key from the dictionary.</para></summary>
		/// <param name="allocator"></param>
		/// <param name="key">The key of the element to be removed from the dictionary.</param>
		/// <param name="value"></param>
		[INLINE(256)]
		public bool Remove(Allocator* allocator, TKey key, out TValue value)
		{
			if (buckets.IsCreated)
			{
				var num = key.GetHashCode() & int.MaxValue;
				var index1 = (int)(num % buckets.Length);
				var index2 = -1;
				// ISSUE: variable of a reference type
				var next = 0;
				for (var index3 = (int)buckets[allocator, index1] - 1; index3 >= 0; index3 = next)
				{
					ref var local = ref entries[allocator, index3];
					next = local.next;
					if (local.hashCode == num)
					{
						if ((local.key.Equals(key) ? 1 : 0) != 0)
						{
							if (index2 < 0)
							{
								buckets[allocator, index1] = (local.next + 1);
							}
							else
							{
								entries[allocator, index2].next = local.next;
							}

							value = local.value;
							local.hashCode = -1;
							local.next = freeList;

							freeList = index3;
							++freeCount;
							return true;
						}
					}

					index2 = index3;
				}
			}

			value = default;
			return false;
		}

		[INLINE(256)]
		public bool TryAdd(Allocator* allocator, TKey key, TValue value)
		{
			return TryInsert(allocator, key, value, InsertionBehavior.none);
		}

		[INLINE(256)]
		public int EnsureCapacity(Allocator* allocator, int capacity)
		{
			Debug.Assert(IsCreated);

			var num = entries.Length;
			if (num >= capacity)
			{
				return num;
			}

			if (buckets.IsCreated == false)
			{
				return Initialize(allocator, capacity);
			}

			var prime = HashHelpers.GetPrime(capacity);
			Resize(allocator, prime);
			return prime;
		}

		[INLINE(256)]
		public DictionaryEnumerator<TKey, TValue> GetEnumerator(Allocator* allocator)
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(allocator), LastIndex);
		}

		[INLINE(256)]
		public new DictionaryEnumerator<TKey, TValue> GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[INLINE(256)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator(Allocator* allocator)
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(allocator), LastIndex);
		}

		[INLINE(256)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator()
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[INLINE(256)]
		public Enumerable<Dictionary<TKey, TValue>.Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetEntryPtr(allocator), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<Dictionary<TKey, TValue>.Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable()
		{
			return new (new (GetEntryPtr(), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new (GetEntryPtr(allocator), LastIndex));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable()
		{
			return new (new (GetEntryPtr(), LastIndex));
		}

		[INLINE(256)]
		IEnumerator<Dictionary<TKey, TValue>.Entry> IEnumerable<Dictionary<TKey, TValue>.Entry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
