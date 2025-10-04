namespace Sapientia.Evaluator
{
	public interface IEvaluator
	{
	}

	public interface IEvaluator<in TContext, out T> : IEvaluator
	{
		T Evaluate(TContext context);
	}
}
