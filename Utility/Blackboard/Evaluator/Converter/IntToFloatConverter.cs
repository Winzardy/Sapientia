using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluator.Blackboard.Converter
{
	[Serializable]
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
	public class IntToFloatConverter : BlackboardConverter<int, float>
	{
		protected override float Convert(int value) => value;
	}
}
