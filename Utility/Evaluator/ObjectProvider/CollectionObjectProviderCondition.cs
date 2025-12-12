using System;

namespace Sapientia.Conditions
{
	/// <summary>
	/// Composite
	/// </summary>
#if CLIENT
	[Sirenix.OdinInspector.TypeRegistryItem(
		"\u2009Collection",
		"/",
		Sirenix.OdinInspector.SdfIconType.Stack,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A,
		priority: 100
	)]
#endif
	[Serializable]
	public sealed class CollectionObjectProviderCondition : CollectionCondition<IObjectsProvider>
	{
	}
}
