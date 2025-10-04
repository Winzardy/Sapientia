#if CLIENT
using UnityEngine;
#endif

namespace Sapientia.BlackboardEvaluator.Converter
{
	public abstract class BlackboardConverter<T1, T2> : BlackboardEvaluator<T2>
	{
		[SerializeReference]
		public BlackboardEvaluator<T1> value;

		protected sealed override T2 OnGet(Blackboard blackboard) => Convert(value.Get(blackboard));

		protected abstract T2 Convert(T1 value);
	}
}
