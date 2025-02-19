using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	/// <summary>
	/// Used only with <see cref="Sapientia.Pooling.ArrayPool{T}"/>
	/// </summary>
	public class Array<T> : IDisposable, IEnumerable<T>
	{
		private T[] _array;
		private int _length;

		public int Length => _length;

		public ref T this[int index] => ref _array[index];

		internal Array()
		{
		}

		public Array(int length) => Initialize(length);

		~Array() => Dispose();

		public void Dispose()
		{
			if (_array != null)
				Clear();

			GC.SuppressFinalize(this);
		}

		internal void Initialize(int length)
		{
			_length = length;
			_array = ArrayPool<T>.Shared.Rent(length);
		}

		internal void Clear()
		{
			_length = 0;

			ArrayPool<T>.Shared.Return(_array, true);
			_array = null;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < _length; i++)
				yield return this[i];
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool Any() => _length > 0;

		public static implicit operator bool(Array<T> array) => array.Any();
	}
}
