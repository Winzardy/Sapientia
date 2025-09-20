namespace Sapientia
{
	public interface IEvaluator<in TContext, out T>
	{
		T Evaluate(TContext context);
	}
}