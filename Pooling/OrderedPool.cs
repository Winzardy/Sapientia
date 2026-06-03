using System;
using System.Collections.Generic;

namespace Sapientia.Pooling
{
	/// <summary>
	/// Object pool that respects creation order and returns
	/// objects in accordance with their creation index.
	///
	/// <code>
	/// Note: Non thread-safe version.
	/// </code>
	/// </summary>
	public class OrderedPool<T> : IObjectPool<T>, IDisposable
		where T : class
	{
		private const int MASK_LIMIT = sizeof(ulong) * 8;

		private readonly IObjectPoolPolicy<T> _policy;

		private T[] _items;
		private Dictionary<T, int> _indexMap;

		private ulong _usageMask;
		private bool[] _usageArray;

		private int _count;
		private int _nextFreeIndex;

		/// <summary>
		/// Total created count.
		/// </summary>
		public int Count { get => _count; }

		public OrderedPool(IObjectPoolPolicy<T> policy, int initialCapacity = 16)
		{
			_policy = policy ?? throw new ArgumentNullException(nameof(policy));

			_items = new T[initialCapacity];
			_indexMap = new Dictionary<T, int>(initialCapacity);
		}

		public void Dispose()
		{
			Clear();

			if (_policy is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		public T Get()
		{
			for (int i = _nextFreeIndex; i < _count; i++)
			{
				if (IsFree(i))
				{
					if (_usageArray != null)
					{
						_usageArray[i] = true;
					}
					else
					{
						_usageMask |= (1UL << i);
					}

					_nextFreeIndex = i + 1;

					var obj = _items[i];
					_policy.OnGet(obj);

					return obj;
				}
			}

			if (_count == _items.Length)
				Resize();

			if (_count >= MASK_LIMIT && _usageArray == null)
			{
				_usageArray = new bool[_items.Length];
				for (int i = 0; i < MASK_LIMIT; i++)
				{
					_usageArray[i] = (_usageMask & (1UL << i)) != 0;
				}
			}

			var created = _policy.Create();

			_items[_count] = created;
			_indexMap.Add(created, _count);

			if (_usageArray != null)
			{
				_usageArray[_count] = true;
			}
			else
			{
				_usageMask |= (1UL << _count);
			}

			_nextFreeIndex = ++_count;

			_policy.OnGet(created);
			return created;
		}

		public void Release(T obj)
		{
			var index = _indexMap[obj];

#if DebugLog
			if (IsFree(index))
				throw new InvalidOperationException($"Object is not in use: {obj}");
#endif

			if (_usageArray != null)
			{
				_usageArray[index] = false;
			}
			else
			{
				_usageMask &= ~(1UL << index);
			}

			if (index < _nextFreeIndex)
				_nextFreeIndex = index;

			_policy.OnRelease(obj);
		}

		public void Clear()
		{
			for (int i = 0; i < _count; i++)
			{
				if (_usageArray != null)
				{
					_usageArray[i] = false;
				}

				_policy.OnDispose(_items[i]);
			}

			_count = 0;
			_usageMask = 0;
			_nextFreeIndex = 0;

			_indexMap.Clear();
		}

		private bool IsFree(int index)
		{
			if (_usageArray != null)
				return !_usageArray[index];

			return (_usageMask & (1UL << index)) == 0;
		}

		private void Resize()
		{
			var newSize = _items.Length * 2;
			Array.Resize(ref _items, newSize);

			if (_usageArray != null)
			{
				Array.Resize(ref _usageArray, newSize);
			}
		}
	}
}
