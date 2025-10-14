using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators
{
#if CLIENT
	[TypeRegistryItem(
		SELECTOR_NAME,
		SELECTOR_CATEGORY,
		SELECTOR_ICON,
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
	public class IntBlackboardRandomEvaluator : IntRangeRandomEvaluator<Blackboard>
	{
		protected override IRandomizer<int> GetRandomizer(Blackboard board) => board.Get<IRandomizer<int>>();
	}
}
