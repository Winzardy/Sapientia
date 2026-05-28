using System;
using System.Collections;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	/// <summary>
	/// Allocates more memory, but makes lookup operations faster.
	/// Also checks for duplicates when adding new values,
	/// and for occurence when removing.
	/// </summary>
	public class HashList<T> : IEnumerable<T>, IReadOnlyList<T>
	{
		public static readonly HashList<T> Empty = new HashList<T>();

		private List<T> _list;
		private HashSet<T> _hashSet;

		public T this[int index] { get => _list[index]; }
		public int Count { get => _list.Count; }

		public event Action<T> Added;
		public event Action<T> Removed;

		public HashList()
		{
			_list = new List<T>();
			_hashSet = new HashSet<T>();
		}

		public HashList(int capacity)
		{
			_list = new List<T>(capacity);
			_hashSet = new HashSet<T>();
		}

		public HashList(IEnumerable<T> collection) : this()
		{
			AddRange(collection);
		}

		public bool Contains(T value)
		{
			return _hashSet.Contains(value);
		}

		public bool Add(T value)
		{
			if (!Contains(value))
			{
				_list.Add(value);
				_hashSet.Add(value);

				Added?.Invoke(value);
				return true;
			}

			return false;
		}

		public void AddRange(IEnumerable<T> range)
		{
			foreach (var item in range)
			{
				Add(item);
			}
		}

		public bool Remove(T value)
		{
			if (Contains(value))
			{
				_list.Remove(value);
				_hashSet.Remove(value);

				Removed?.Invoke(value);
				return true;
			}

			return false;
		}

		public int IndexOf(T value)
		{
			return Contains(value) ?
				_list.IndexOf(value) :
				-1;
		}

		public bool WithinBounds(int index)
		{
			return _list.WithinBounds(index);
		}

		public void Clear()
		{
			_list.Clear();
			_hashSet.Clear();
		}

		public IEnumerator<T> GetEnumerator()
			=> _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
