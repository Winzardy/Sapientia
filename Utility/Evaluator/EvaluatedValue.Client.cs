#if CLIENT
namespace Sapientia.Evaluators
{
	public partial struct EvaluatedValue<TContext, TValue> : IEvaluatedValue
	{
		public IEvaluator Evaluator => evaluator;

		void IEvaluatedValue.ToConstantMode()
		{
			// if (evaluator is IConstantEvaluator<TValue> constantEvaluator)
			// 	value = constantEvaluator.Value;
			evaluator = null;
		}

		void IEvaluatedValue.ToEvaluatorMode()
			=> evaluator = new IfElseEvaluator<TContext, TValue>();
	}

	public interface IEvaluatedValue
	{
		public IEvaluator Evaluator { get; }
		public void ToConstantMode();
		public void ToEvaluatorMode();
	}
}
#endif
