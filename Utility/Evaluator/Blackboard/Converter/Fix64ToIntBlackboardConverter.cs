using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators.Converter
{
#if CLIENT
	[TypeRegistryItem(
		"\u2009Float To Int",
		"Convert",
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
	public class Fix64ToIntBlackboardConverter : Fix64ToIntConverter<Blackboard>
	{
	}
}
