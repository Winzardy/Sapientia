using Sapientia.Deterministic;

namespace Sapientia.BlackboardEvaluator
{
	public static class BlackboardConstantFactory
	{
		public static BlackboardEvaluator<T> Create<T>(T value)
		{
			BlackboardConstantEvaluator<T>? evaluator = null;
			if (value is int i)
				evaluator = new IntConstantEvaluator(i) as BlackboardConstantEvaluator<T>;

			if (value is Fix64 f)
				evaluator = new Fix64ConstantEvaluator(f) as BlackboardConstantEvaluator<T>;

			evaluator ??= new BlackboardConstantEvaluator<T>(default);
			return evaluator;
		}
	}
}
