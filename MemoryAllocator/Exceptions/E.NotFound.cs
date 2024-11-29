namespace Sapientia.MemoryAllocator
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
		public class NotFoundException : System.Exception
		{
			public NotFoundException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static System.Exception Throw(string obj)
			{
				return new OutOfRangeException($"Object was not found {obj}");
			}
		}
	}

	public static partial class E
	{
		[HIDE_CALLSTACK]
		public static System.Exception NOT_FOUND(string obj)
		{
			return NotFoundException.Throw(obj);
		}
	}
}
