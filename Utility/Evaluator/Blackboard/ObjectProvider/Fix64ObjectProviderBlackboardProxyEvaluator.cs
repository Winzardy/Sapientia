using System;
using Sapientia.Deterministic;

namespace Sapientia.Evaluators
{
#if CLIENT
	[Sirenix.OdinInspector.TypeRegistryItem(
		IProxyEvaluator.SELECTOR_NAME,
		"/",
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
	public class Fix64ObjectProviderBlackboardProxyEvaluator : ObjectProviderBlackboardProxyEvaluator<Fix64>
	{
	}
}
