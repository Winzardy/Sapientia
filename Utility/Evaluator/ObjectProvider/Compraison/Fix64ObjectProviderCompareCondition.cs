using System;

namespace Sapientia.Conditions.Comparison
{
#if CLIENT
	[Sirenix.OdinInspector.TypeRegistryItem(
		SELECTOR_NAME,
		SELECTOR_CATEGORY,
		SELECTOR_ICON,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	[Serializable]
	public sealed class Fix64ObjectProviderCompareCondition : Fix64CompareCondition<IObjectsProvider>
	{
	}
}
