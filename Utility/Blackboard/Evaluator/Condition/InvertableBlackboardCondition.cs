using System;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract class InvertableBlackboardCondition : BlackboardCondition
	{
		public bool invert;

		public override bool IsFulfilled(Blackboard blackboard) => invert
			? !base.IsFulfilled(blackboard)
			: base.IsFulfilled(blackboard);
	}
}
