using System;

namespace Sapientia.Evaluators
{
#if CLIENT
	[Sirenix.OdinInspector.TypeRegistryItem(
		"\u2009Arithmetic Operation",
		"Math",
		Sirenix.OdinInspector.SdfIconType.PlusSlashMinus,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	[Serializable]
	public class Fix64VsFix64ObjectProviderArithmeticOperation : Fix64VsFix64ArithmeticOperation<IObjectsProvider>
	{
	}
}
