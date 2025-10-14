using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class IntRangeRandomEvaluator<TContext> : RangeRandomEvaluator<TContext, int>
	{
		public IntRangeRandomEvaluator() : this(0, 1)
		{
		}

		public IntRangeRandomEvaluator(int min, int max) : base(min, max)
		{
		}
	}
}
