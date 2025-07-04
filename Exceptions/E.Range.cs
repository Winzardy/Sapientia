using System.Diagnostics;

namespace Sapientia
{
#if UNITY_EDITOR
	using BURST_DISCARD = Unity.Burst.BurstDiscardAttribute;
	using HIDE_CALLSTACK = UnityEngine.HideInCallstackAttribute;
#else
	using BURST_DISCARD = PlaceholderAttribute;
	using HIDE_CALLSTACK = PlaceholderAttribute1;
#endif

	public partial class E
	{
		public class OutOfRangeException : System.Exception
		{
			public OutOfRangeException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static void Throw(int index, int startIndex, int count)
			{
				ThrowNotBurst(index, startIndex, count);
				throw new OutOfRangeException("Out of range");
			}

			[BURST_DISCARD]
			[HIDE_CALLSTACK]
			private static void ThrowNotBurst(int index, int startIndex, int count) =>
				throw new OutOfRangeException($"index {index} out of range {startIndex}..{count}");
		}
	}

	public static partial class E
	{
		[Conditional(DEBUG)]
		[HIDE_CALLSTACK]
		public static void RANGE(in int index, int startIndex, in int length)
		{
			if (index >= startIndex && index < length) return;
			OutOfRangeException.Throw(index, startIndex, length);
		}

		[Conditional(DEBUG)]
		[HIDE_CALLSTACK]
		public static void RANGE(in int index, uint startIndex, in uint length)
		{
			if (index >= startIndex && index < length) return;
			OutOfRangeException.Throw(index, (int)startIndex, (int)length);
		}

		[Conditional(DEBUG)]
		[HIDE_CALLSTACK]
		public static void RANGE(in uint index, uint startIndex, in uint length)
		{
			if (index >= startIndex && index < length) return;
			OutOfRangeException.Throw((int)index, (int)startIndex, (int)length);
		}

		[Conditional(DEBUG)]
		[HIDE_CALLSTACK]
		public static void RANGE_INVERSE(uint index, uint length)
		{
			if (index >= length) return;
			OutOfRangeException.Throw((int)index, 0, (int)length);
		}

		[Conditional(DEBUG)]
		[HIDE_CALLSTACK]
		public static void OUT_OF_RANGE()
		{
			OutOfRangeException.Throw(0, 0, 0);
		}
	}
}
