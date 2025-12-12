using System;

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
	public class IntObjectProviderBlackboardProxyEvaluator : ObjectProviderBlackboardProxyEvaluator<int>
	{
	}
}
