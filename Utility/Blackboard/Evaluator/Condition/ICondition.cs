using Sapientia.BlackboardEvaluator;

namespace Sapientia
{
	internal interface ICondition : IBlackboardEvaluator<bool>
	{
		public bool IsFulfilled(Blackboard blackboard);
	}
}
