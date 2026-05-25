#nullable disable
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
		priority: 1
	)]
#endif
	[Serializable]
	public sealed class IntBlackboardValueEvaluator : BlackboardValueEvaluator<int>
	{
	}
}
