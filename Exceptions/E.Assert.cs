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
		public class AssertException : System.Exception
		{
			public AssertException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static void Throw(string str)
			{
				ThrowNotBurst(str);
				throw new AssertException("Internal exception");
			}

			[BURST_DISCARD]
			[HIDE_CALLSTACK]
			private static void ThrowNotBurst(string str) => throw new AssertException(str);
		}
	}

	public static partial class E
	{
		[Conditional(DEBUG)]
		[HIDE_CALLSTACK]
		public static void ASSERT(bool condition, string str = null!)
		{
			if (condition)
				return;
			AssertException.Throw(str);
		}
	}
}
