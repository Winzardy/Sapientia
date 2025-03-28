namespace Sapientia.MemoryAllocator
{
	using System.Diagnostics;
#if UNITY_EDITOR
	using BURST_DISCARD = Unity.Burst.BurstDiscardAttribute;
	using HIDE_CALLSTACK = UnityEngine.HideInCallstackAttribute;
#else
	using BURST_DISCARD = PlaceholderAttribute;
	using HIDE_CALLSTACK = PlaceholderAttribute1;
#endif

	public partial class E
	{
		public class SizeEqualsException : System.Exception
		{
			public SizeEqualsException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static void Throw()
			{
				throw new OutOfRangeException("Size must be equals");
			}
		}
	}

	public static partial class E
	{
		[Conditional(ENABLE_EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void SIZE_EQUALS(uint sizeT, uint sizeStored)
		{
			if (sizeT == sizeStored) return;
			SizeEqualsException.Throw();
		}
	}
}
