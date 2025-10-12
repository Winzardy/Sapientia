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
	public class IntBlackboardRandomEvaluator : IntRandomEvaluator<Blackboard>
	{
		protected override IRandomizer<int> GetRandomizer(Blackboard board) => board.Get<IRandomizer<int>>();
	}
}
