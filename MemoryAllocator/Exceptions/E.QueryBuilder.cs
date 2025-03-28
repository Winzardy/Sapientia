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
		public class QueryBuilderException : System.Exception
		{
			public QueryBuilderException(string str) : base(str)
			{
			}

			[HIDE_CALLSTACK]
			public static void Throw(string str)
			{
				ThrowNotBurst(str);
				throw new QueryBuilderException("Internal collection exception");
			}

			[BURST_DISCARD]
			[HIDE_CALLSTACK]
			private static void ThrowNotBurst(string str) =>
				throw new QueryBuilderException($"{Format(str)}");
		}
	}

	public static partial class E
	{
		[Conditional(ENABLE_EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void QUERY_BUILDER_AS_JOB(bool asJob)
		{
			if (asJob == true)
				QueryBuilderException.Throw("Query Builder can't use this method because it is in AsJob mode");
		}

		[Conditional(ENABLE_EXCEPTIONS)]
		[HIDE_CALLSTACK]
		public static void QUERY_BUILDER_PARALLEL_FOR(uint parallelForBatch)
		{
			if (parallelForBatch > 0u)
				QueryBuilderException.Throw("Query Builder can't use this method because it is in ParallelFor mode");
		}
	}
}
