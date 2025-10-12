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
	public abstract class IntRandomEvaluator<TContext> : RandomEvaluator<TContext,int>
	{
		public IntRandomEvaluator() : this(0, 1)
		{
		}

		public IntRandomEvaluator(int min, int max) : base(min, max)
		{
		}
	}
}
