using System;

namespace Sapientia
{
	[Serializable]
	public abstract class Condition : ICondition
	{
		public bool invert;

		public bool Evaluate(Blackboard blackboard)
		{
			return invert ? !IsMet(blackboard) : IsMet(blackboard);
		}

		protected abstract bool IsMet(Blackboard blackboard);
	}
}