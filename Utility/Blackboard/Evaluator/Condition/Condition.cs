using System;
using Sapientia.Evaluator;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract partial class Condition : ICondition
	{
		public bool invert;

		protected internal bool Evaluate(Blackboard blackboard) => IsFulfilled(blackboard);

		bool IEvaluator<Blackboard, bool>.Evaluate(Blackboard blackboard)
			=> IsFulfilled(blackboard);

		public bool IsFulfilled(Blackboard blackboard) => invert ? !OnEvaluate(blackboard) : OnEvaluate(blackboard);

		protected abstract bool OnEvaluate(Blackboard blackboard);
	}
}
