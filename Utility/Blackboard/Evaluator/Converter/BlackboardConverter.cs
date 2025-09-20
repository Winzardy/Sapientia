using UnityEngine;

namespace Sapientia
{
	public abstract class BlackboardConverter<T1, T2> : IBlackboardEvaluator<T2>
	{
		[SerializeReference]
		public IBlackboardEvaluator<T1> value;

		public T2 Evaluate(Blackboard context) => Convert(value.Evaluate(context));

		protected abstract T2 Convert(T1 value);
	}
}