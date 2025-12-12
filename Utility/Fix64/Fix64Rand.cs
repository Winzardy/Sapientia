using System;

namespace Sapientia.Deterministic.Utility
{
	public class Fix64Rand
	{
		private ulong _state;
		private ulong _inc;

		public State Current
		{
			get
			{
				unchecked
				{
					return new State
					{
						a = (int) (_state & 0xffffffff),
						b = (int) (_state >> 32),
						c = (int) (_inc & 0xffffffff),
						d = (int) (_inc >> 32),
					};
				}
			}
			set
			{
				unchecked
				{
					_state = (uint) value.a | (ulong) (uint) value.b << 32;
					_inc = (uint) value.c | (ulong) (uint) value.d << 32;
				}
			}
		}

		public Fix64 Next() => NextFix64();
		public int NextInt() => (int) NextFix64();

		public Fix64Rand(int seed) : this((ulong) seed)
		{
		}

		public Fix64Rand(ulong seed)
		{
			ulong x = seed;
			_state = NextSplitMix64(ref x);
			_inc = NextSplitMix64(ref x);
		}

		public Fix64Rand(State state)
		{
			this.Current = state;
		}

		internal Fix64 NextFix64() => Fix64.FromRaw(NextUInt32());

		public Fix64 Next(Fix64 minInclusive, Fix64 maxExclusive)
		{
			if (minInclusive > maxExclusive)
			{
				(minInclusive, maxExclusive) = (maxExclusive, minInclusive);
			}

			return minInclusive + Next() * (maxExclusive - minInclusive);
		}

		public int Next(int minInclusive, int maxExclusive)
		{
			if (minInclusive == maxExclusive)
				return minInclusive;

			if (minInclusive > maxExclusive)
			{
				(minInclusive, maxExclusive) = (maxExclusive, minInclusive);
			}

			uint r = NextUnbiased((uint) (maxExclusive - minInclusive));
			return (int) (minInclusive + r);
		}

		public Fix64 NextInclusive(Fix64 minInclusive, Fix64 maxInclusive)
		{
			if (minInclusive > maxInclusive)
			{
				(minInclusive, maxInclusive) = (maxInclusive, minInclusive);
			}

			long rawValue = (maxInclusive - minInclusive).RawValue;
			Fix64 fp1;
			if (rawValue < uint.MaxValue)
			{
				fp1 = Fix64.FromRaw(NextUnbiased((uint) rawValue + 1U));
			}
			else
			{
				uint num = NextUInt32();
				fp1 = Fix64.FromRaw((long) NextUnbiased((uint) (rawValue >> 32) + 1U) << 32 | num);
			}

			return minInclusive + fp1;
		}

		public int NextInclusive(int minInclusive, int maxInclusive)
		{
			if (minInclusive == maxInclusive)
				return minInclusive;

			if (minInclusive > maxInclusive)
			{
				(minInclusive, maxInclusive) = (maxInclusive, minInclusive);
			}

			uint max = (uint) (maxInclusive - minInclusive + 1);

			if (max == 0U)
				return (int) NextUInt32();

			uint r = NextUnbiased(max);
			return (int) (minInclusive + r);
		}

		// Melissa O'Neill's PCG (https://www.pcg-random.org/)
		internal uint NextUInt32()
		{
			ulong oldstate = _state;

			// Advance internal state.
			_state = (ulong) ((long) oldstate * 6364136223846793005L + ((long) _inc | 1L));

			// Calculate output function (XSH RR), uses old state for max ILP
			uint xorshifted = (uint) ((oldstate >> 18 ^ oldstate) >> 27);
			int rot = (int) (oldstate >> 59);
			return xorshifted >> rot | xorshifted << -rot;
		}

		private uint NextUnbiased(uint max)
		{
			uint num1 = (uint) -(int) max % max;
			uint num2;
			do
			{
				num2 = NextUInt32();
			} while (num2 < num1);

			return num2 % max;
		}

		// https://rosettacode.org/wiki/Pseudo-random_numbers/Splitmix64
		private static ulong NextSplitMix64(ref ulong state)
		{
			state += 0x9E3779B97F4A7C15UL;
			ulong z = state;
			z = (z ^ z >> 30) * 0xbf58476d1ce4e5b9UL;
			z = (z ^ z >> 27) * 0x94d049bb133111ebUL;
			return z ^ z >> 31;
		}

		// // https://stackoverflow.com/a/50746409/423959
		// public Fix64Vec2 NextInsideUnitCircle()
		// {
		//  var radius = Fix64.Sqrt(Next());
		//  var radians = Next() * Fix64.PiTimes2;
		//  var sin = Fix64.FastSin(radians);
		//  var cos = Fix64.FastCos(radians);
		//  return new Fix64Vec2(radius * cos, radius * sin);
		// }
		//
		// public Fix64Vec2 NextOnUnitCircle()
		// {
		//  var radians = Next() * Fix64._2 * Fix64.Pi;
		//  var sin = Fix64.FastSin(radians);
		//  var cos = Fix64.FastCos(radians);
		//  return new Fix64Vec2(cos, sin);
		// }

		[Serializable]
		public struct State
		{
			public int a, b, c, d;

			public bool IsDefault =>
				a == 0 &&
				b == 0 &&
				c == 0 &&
				d == 0;

			public override string ToString() => $"a: {a}, b: {b}, c: {c}, d: {d}";
		}
	}
}
