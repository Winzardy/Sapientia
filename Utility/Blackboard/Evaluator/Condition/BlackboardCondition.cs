using System;
using Sapientia.Evaluator;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract partial class BlackboardCondition : IBlackboardCondition
	{
		bool IEvaluator<Blackboard, bool>.Evaluate(Blackboard blackboard)
			=> IsFulfilled(blackboard);

		public virtual bool IsFulfilled(Blackboard blackboard) =>
			OnEvaluate(blackboard);

		protected internal bool Evaluate(Blackboard blackboard) => IsFulfilled(blackboard);

		protected abstract bool OnEvaluate(Blackboard blackboard);
	}
}
