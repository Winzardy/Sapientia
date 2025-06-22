using System;

namespace Sapientia
{
	public interface IMinMax
	{
	}


	[Serializable]
	public partial struct Range<T> : IMinMax, IContainer<T>
	{
		public T min;
		public T max;

		public Range(T min, T max)
		{
			this.min = min;
			this.max = max;
		}

		public static implicit operator Range<T>((T min, T max) tuple) => new(tuple.min, tuple.max);
		public static implicit operator Range<T>(T max) => new Range<T>(default, max);
	}

	public static class RangeUtility
	{
		public static bool Contains(this in Range<float> range, float value, bool maxInclusive = false)
			=> value >= range.min &&
				(maxInclusive ? value < range.max : value <= range.max);

		public static bool Contains(this in Range<int> range, int value, bool maxInclusive = false)
			=> value >= range.min &&
				(maxInclusive ? value < range.max : value <= range.max);
	}
}
