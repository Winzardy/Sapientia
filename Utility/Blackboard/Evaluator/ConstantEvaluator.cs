namespace Sapientia
{
	public abstract class ConstantEvaluator<TContext, T> : IEvaluator<TContext, T>
	{
		public T value;

		public T Evaluate(TContext _) => value;
	}
}