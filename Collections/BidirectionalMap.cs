using System;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	public class BidirectionalMap<TFirst, TSecond> : IDisposable
	{
		private Dictionary<TFirst, TSecond> _firstToSecond;
		private Dictionary<TSecond, TFirst> _secondToFirst;

		public ICollection<TFirst> FirstValues => _secondToFirst.Values;
		public ICollection<TSecond> SecondValues => _firstToSecond.Values;

		public int Count => _firstToSecond.Count;

		public TSecond this[TFirst first]
		{
			get => _firstToSecond[first];
			set
			{
				_firstToSecond[first] = value;
				_secondToFirst[value] = first;
			}
		}

		public TFirst this[TSecond second]
		{
			get => _secondToFirst[second];
			set
			{
				_secondToFirst[second] = value;
				_firstToSecond[value] = second;
			}
		}

		public BidirectionalMap()
		{
			_firstToSecond = new Dictionary<TFirst, TSecond>();
			_secondToFirst = new Dictionary<TSecond, TFirst>();
		}

		public BidirectionalMap(int count)
		{
			_firstToSecond = new Dictionary<TFirst, TSecond>(count);
			_secondToFirst = new Dictionary<TSecond, TFirst>(count);
		}

		public void Dispose()
		{
			_firstToSecond = null;
			_secondToFirst = null;
		}

		public void Clear()
		{
			_firstToSecond.Clear();
			_secondToFirst.Clear();
		}

		public void Add(TFirst first, TSecond second)
		{
			if (_firstToSecond.ContainsKey(first) || _secondToFirst.ContainsKey(second))
			{
				throw new ArgumentException("Detected duplicate for bidirectional map.");
			}

			_firstToSecond.Add(first, second);
			_secondToFirst.Add(second, first);
		}

		public TSecond GetByFirst(TFirst first) => _firstToSecond[first];

		public TFirst GetBySecond(TSecond second) => _secondToFirst[second];

		public bool TryGetValue(TFirst first, out TSecond second) => _firstToSecond.TryGetValue(first, out second);

		public bool TryGetValue(TSecond second, out TFirst first) => _secondToFirst.TryGetValue(second, out first);

		public bool TryGetByFirst(TFirst first, out TSecond second) => _firstToSecond.TryGetValue(first, out second);

		public bool TryGetBySecond(TSecond second, out TFirst first) => _secondToFirst.TryGetValue(second, out first);

		public bool IsNullOrEmpty() => _firstToSecond.IsNullOrEmpty() || _secondToFirst.IsNullOrEmpty();
	}
}
