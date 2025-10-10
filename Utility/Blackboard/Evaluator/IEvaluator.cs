namespace Sapientia.Evaluator
{
	public interface IEvaluator
	{
		public const float R = 0.6f;
		public const float G = 1f;
		public const float B = 0.6f;
		public const float A = 1;
	}

	public interface IEvaluator<in TContext, out T> : IEvaluator
	{
		T Evaluate(TContext context);
	}
}
