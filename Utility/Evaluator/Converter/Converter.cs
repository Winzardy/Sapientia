#if CLIENT
using UnityEngine;
#endif

namespace Sapientia.Evaluators.Converter
{
	public abstract class Converter<TContext, T1, T2> : Evaluator<TContext, T2>
	{
		[SerializeReference]
		public Evaluator<TContext, T1> value;

		protected sealed override T2 OnEvaluate(TContext context) => Convert(value.Evaluate(context));

		protected abstract T2 Convert(T1 value);
	}
}
