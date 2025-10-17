using System;

namespace Sapientia.Conditions
{
#if CLIENT
	[Sirenix.OdinInspector.TypeRegistryItem(
		"\u2009Boolean Operation",
		"/",
		Sirenix.OdinInspector.SdfIconType.CodeSlash,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	[Serializable]
	public sealed class BinaryObjectProviderCondition : BinaryCondition<IObjectsProvider>
	{
	}
}
