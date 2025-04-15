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
		public class CollectionInternalException : System.Exception
		{
			public CollectionInternalException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static void Throw(string str)
			{
				ThrowNotBurst(str);
				throw new CollectionInternalException("Internal collection exception");
			}

			[BURST_DISCARD]
			[HIDE_CALLSTACK]
			private static void ThrowNotBurst(string str) => throw new CollectionInternalException($"{Format(str)}");
		}
	}

	public static partial class E
	{
		[HIDE_CALLSTACK]
		public static void ADDING_DUPLICATE()
		{
			CollectionInternalException.Throw("Duplicate adding");
		}

		[HIDE_CALLSTACK]
		public static void NOT_FOUND(string obj)
		{
			CollectionInternalException.Throw(obj);
		}
	}
}
