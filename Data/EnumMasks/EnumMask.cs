using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Sapientia.Extensions;

namespace Sapientia.Data
{
#if UNITY_EDITOR
	public interface IEnumMask
	{
	}
#endif

	[DebuggerTypeProxy(typeof(EnumMask<>.EnumMaskProxy))]
	[Serializable]
	public struct EnumMask<T>
#if UNITY_EDITOR
		: IEnumMask
#endif
		where T : unmanaged, Enum
	{
		public const int BitsCount = 32;

		public static readonly EnumMask<T> All = new() {mask = ~0};

		public int mask;

		[System.Diagnostics.Conditional(E.DEBUG)]
		private static void AssertValue(int value)
		{
			// Вместо Assert, т.к. создание строки просаживает производительность в редакторе
			if (value is < BitsCount and >= 0)
				return;
			E.AssertException.Throw($"Количество бит выходит за пределы рабочего диапазона [0 >= (value: {value}) < {BitsCount}]");
		}

		public EnumMask(params T[] values) : this()
		{
			for (var i = 0; i < values.Length; i++)
				Add(values[i]);
		}

		public static EnumMask<T> Create(T value)
		{
			var result = new EnumMask<T>();
			result.Add(value);
			return result;
		}

		public bool HasNothing()
		{
			return mask == 0;
		}

		public bool HasAnything()
		{
			return mask == 0;
		}

		public readonly bool Has(T value)
		{
			return Has(value.ToInt());
		}

		public readonly bool HasOnly(T value)
		{
			return HasOnly(value.ToInt());
		}

		public readonly bool Has(int value)
		{
			AssertValue(value);
			return (mask & (1 << value)) != 0;
		}

		public readonly bool HasOnly(int value)
		{
			AssertValue(value);
			var valueMask = 1 << value;
			return ((mask & (1 << value)) != 0) && ((mask & ~valueMask) == 0);
		}

		public readonly bool Has(EnumMask<T> value)
		{
			return (value.mask & mask) != 0;
		}

		public void Set(T value, bool add)
		{
			Set(value.ToInt(), add);
		}

		public void Set(int value, bool add)
		{
			AssertValue(value);
			if (add)
				Add(value);
			else
				Remove(value);
		}

		public void Add(T value)
		{
			Add(value.ToInt());
		}

		public void Add(int value)
		{
			AssertValue(value);
			mask |= (1 << value);
		}

		public void Remove(T value)
		{
			Remove(value.ToInt());
		}

		public void Remove(int value)
		{
			AssertValue(value);
			mask &= ~(1 << value);
		}

		public static EnumMask<T> operator |(EnumMask<T> a, EnumMask<T> b)
		{
			return new EnumMask<T> {mask = a.mask | b.mask};
		}

		public static EnumMask<T> operator &(EnumMask<T> a, EnumMask<T> b)
		{
			return new EnumMask<T> {mask = a.mask & b.mask};
		}

		public static implicit operator EnumMask<T>(T value)
		{
			var mask = new EnumMask<T>();
			mask.Add(value);
			return mask;
		}

		public static implicit operator uint(EnumMask<T> value)
		{
			return value.mask.As<int, uint>();
		}

		public static implicit operator short(EnumMask<T> value)
		{
			return unchecked((short)value.mask);
		}

		public static implicit operator ushort(EnumMask<T> value)
		{
			return unchecked((ushort)value.mask);
		}

		public static implicit operator EnumMask<T>(uint value)
		{
			var mask = value.As<uint, int>();
			return new EnumMask<T>
			{
				mask = mask,
			};
		}

		public static implicit operator UndefinedEnumMask(EnumMask<T> value)
		{
			return new UndefinedEnumMask()
			{
				mask = value.mask,
			};
		}

		public static implicit operator EnumMask<T>(UndefinedEnumMask value)
		{
			return new EnumMask<T>()
			{
				mask = value.mask,
			};
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append($"{typeof(T).Name}: ");
			for (var i = 0; i < EnumValues<T>.ENUM_LENGHT; i++)
			{
				var has = Has(EnumValues<T>.VALUES[i]);
				var name = EnumNames<T>.NAMES[i];
				sb.Append($"{name}-{has}; ");
			}
			return sb.ToString();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public readonly ref struct Enumerable
		{
			private readonly Enumerator _enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerable(Enumerator enumerator)
			{
				_enumerator = enumerator;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator GetEnumerator()
			{
				return _enumerator;
			}
		}

		public ref struct Enumerator
		{
			private readonly EnumMask<T> _mask;
			private int _index;

			public T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => EnumValues<T>.VALUES[_index];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator(EnumMask<T> mask)
			{
				_mask = mask;
				_index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				do
				{
					if (++_index >= EnumValues<T>.ENUM_LENGHT)
						return false;
				}
				while (!_mask.Has(EnumValues<T>.VALUES[_index]));

				return true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				this = default;
			}
		}

		private class EnumMaskProxy
		{
			private EnumMask<T> _mask;

			public EnumMaskProxy(EnumMask<T> mask)
			{
				_mask = mask;
			}

			public (T value, bool has)[] Items
			{
				get
				{
#if DEBUG
					var arr = new (T value, bool has)[EnumValues<T>.ENUM_LENGHT];
					for (var i = 0; i < arr.Length; ++i)
					{
						var value = EnumValues<T>.VALUES[i];
						var has = _mask.Has(EnumValues<T>.VALUES[i]);
						arr[i] = (value, has);
					}

					return arr;
#else
					return Array.Empty<(T value, bool has)>();
#endif
				}
			}
		}
	}
}
