using System;
using Sapientia.Deterministic;
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
	public class Fix64BlackboardRandomEvaluator : Fix64RangeRandomEvaluator<Blackboard>
	{
		protected override IRandomizer<Fix64> GetRandomizer(Blackboard board) => board.Get<IRandomizer<Fix64>>();
	}
}
