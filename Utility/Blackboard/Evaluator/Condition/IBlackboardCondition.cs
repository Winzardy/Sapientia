using Sapientia.Evaluator.Blackboard;

namespace Sapientia
{
	public interface IBlackboardCondition : IBlackboardEvaluator<bool>
	{
		public bool IsFulfilled(Blackboard blackboard);
	}
}
