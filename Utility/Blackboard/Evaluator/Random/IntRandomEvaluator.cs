using System;
using Sapientia.Evaluator.Blackboard;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluator.Blackboard
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
	public class IntRandomEvaluator : BlackboardRandomEvaluator<int>
	{
		public IntRandomEvaluator() : this(0, 1)
		{
		}

		public IntRandomEvaluator(int min, int max) : base(min, max)
		{
		}
	}
}
