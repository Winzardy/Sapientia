namespace Sapientia.Evaluators
{
	public abstract partial class Evaluator<TContext, TValue>
	{
		public const float R = IEvaluator.R;
		public const float G = IEvaluator.G;
		public const float B = IEvaluator.B;
		public const float A = IEvaluator.A;

		public const int OPERATOR_WIDTH = ICondition.OPERATOR_WIDTH;
	}
}
