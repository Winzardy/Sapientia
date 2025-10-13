using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators
{
#if CLIENT
	[TypeRegistryItem(
		"\u2009Range",
		"/",
		SdfIconType.DiamondHalf,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A,
		priority: 99
	)]
#endif
	[Serializable]
	public abstract class IntRangeRandomEvaluator<TContext> : RangeRandomEvaluator<TContext,int>
	{
		public IntRangeRandomEvaluator() : this(0, 1)
		{
		}

		public IntRangeRandomEvaluator(int min, int max) : base(min, max)
		{
		}
	}
}
