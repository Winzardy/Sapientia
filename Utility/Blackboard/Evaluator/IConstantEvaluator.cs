namespace Sapientia.Evaluator
{
	public interface IConstantEvaluator
	{
	}

	public interface IConstantEvaluator<T> : IConstantEvaluator
	{
		public ref readonly T Value { get; }
	}


}
