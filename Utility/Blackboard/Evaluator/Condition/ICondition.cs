using Sapientia.BlackboardEvaluator;

namespace Sapientia
{
	internal interface ICondition : IBlackboardEvaluator<bool>
	{
		public bool IsMet(Blackboard blackboard);
	}
}
