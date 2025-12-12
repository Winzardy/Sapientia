using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators.Converter
{
#if CLIENT
	[TypeRegistryItem(
		"\u2009Int To Float",
		"Common",
		SdfIconType.ArrowBarRight,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	[Serializable]
	public class IntToFix64BlackboardConverter : IntToFix64Converter<Blackboard>
	{
	}
}
