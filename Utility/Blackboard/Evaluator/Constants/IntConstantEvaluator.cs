using System;
using Sapientia;
using Sapientia.BlackboardEvaluator;
#if CLIENT
using Sirenix.OdinInspector;
#endif

#if CLIENT
[TypeRegistryItem(
	"\u2009Constant",
	"",
	SdfIconType.DiamondFill,
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
public class IntConstantEvaluator : BlackboardConstantEvaluator<int>
{
	public IntConstantEvaluator() : this(0)
	{
	}

	public IntConstantEvaluator(int value) : base(value)
	{
	}

	public override string ToString() => value > 1 ? value.ToString() : string.Empty;
}
