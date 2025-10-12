namespace Sapientia
{
	public interface IEvaluator
	{
		public const float R = 0.6f;
		public const float G = 1f;
		public const float B = 0.6f;
		public const float A = 1;

		public const string ARITHMETIC_OPERATOR_ADD = "+";
		public const string ARITHMETIC_OPERATOR_SUBTRACT = "\u2212";
		public const string ARITHMETIC_OPERATOR_DIVIDE = "\u00f7";
		public const string ARITHMETIC_OPERATOR_MULTIPLY = "\u00d7";
	}

	public interface IEvaluator<in TContext, out T> : IEvaluator
	{
		T Evaluate(TContext context);
	}
}
