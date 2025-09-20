namespace Sapientia
{
	internal interface ICondition : IEvaluator<Blackboard, bool>
	{
		public bool IsMet(Blackboard blackboard) => Evaluate(blackboard);
	}
}
