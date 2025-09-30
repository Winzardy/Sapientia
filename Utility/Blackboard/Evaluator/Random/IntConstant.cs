using System;
using Sapientia.BlackboardEvaluator;
#if CLIENT
using Sirenix.OdinInspector;
#endif

#if CLIENT
[TypeRegistryItem(
	"\u2009Range",
	"",
	SdfIconType.DiamondHalf,
	darkIconColorR: R, darkIconColorG: G,
	darkIconColorB: B,
	darkIconColorA: A,
	lightIconColorR: R, lightIconColorG: G,
	lightIconColorB: B,
	lightIconColorA: A,
	priority: 100
)]
#endif
[Serializable]
public class IntRandomEvaluator : BlackboardRandomEvaluator<int>
{
	public IntRandomEvaluator() : this(0, 1)
	{
	}

	public IntRandomEvaluator(int min, int max) : base(min, max)
	{
	}
}
