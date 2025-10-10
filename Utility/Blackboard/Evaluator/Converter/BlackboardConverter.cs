#if CLIENT
using UnityEngine;
#endif

namespace Sapientia.Evaluator.Blackboard.Converter
{
	public abstract class BlackboardConverter<T1, T2> : BlackboardEvaluator<T2>
	{
		[SerializeReference]
		public BlackboardEvaluator<T1> value;

		protected sealed override T2 OnGet(Sapientia.Blackboard blackboard) => Convert(value.Get(blackboard));

		protected abstract T2 Convert(T1 value);
	}
}
