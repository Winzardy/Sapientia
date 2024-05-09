using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections
{
	[Serializable]
	public class EnumArray<TEnum, TValue> : IEnumerable<TValue> where TEnum : unmanaged, Enum
	{
		private TValue[] _values = new TValue[EnumValues<TEnum>.ENUM_LENGHT];

		public TValue[] Values => _values;

		public ref TValue this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[index];
		}

		public unsafe ref TValue this[TEnum enumValue]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[GetIndexOf(enumValue)];
		}

		public int Lenght
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _values.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int GetIndexOf(TEnum enumValue)
		{
			return *(int*)(&enumValue);
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return new Enumerator(_values);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public struct Enumerator : IEnumerator<TValue>
		{
			private readonly TValue[] _array;
			private int _index;

			public TValue Current => _array[_index];

			object IEnumerator.Current => Current!;

			internal Enumerator(TValue[] array)
			{
				_array = array;
				_index = -1;
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _array.Length;
			}

			public void Reset()
			{
				_index = -1;
			}

			public void Dispose() {}
		}
	}
}