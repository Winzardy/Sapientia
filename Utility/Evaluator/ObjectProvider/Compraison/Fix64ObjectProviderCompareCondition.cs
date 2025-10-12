using System;

namespace Sapientia.Conditions.Comparison
{
#if CLIENT
	[Sirenix.OdinInspector.TypeRegistryItem(
		"\u2009Float Comparison",
		"Comparison",
		Sirenix.OdinInspector.SdfIconType.ArrowLeftRight,
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
