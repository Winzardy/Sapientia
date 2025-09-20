using System;

namespace Sapientia
{
	[Serializable]
	public class BlackboardConstant<T> : ConstantEvaluator<Blackboard, T>, IBlackboardEvaluator<T>
	{
	}
}