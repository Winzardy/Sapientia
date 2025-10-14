using System;
using Sapientia.Conditions;
using Sapientia.Evaluators;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia
{
#if CLIENT
	[TypeRegistryItem(
		IProxyEvaluator.SELECTOR_NAME,
		IProxyEvaluator.SELECTOR_GROUP,
		IProxyEvaluator.SELECTOR_ICON,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	[Serializable]
	public class ObjectProviderBlackboardProxyEvaluator : ProxyCondition<Blackboard, IObjectsProvider>
	{
		protected override IObjectsProvider Convert(Blackboard board) => board.Get<IObjectsProvider>();
	}
}
