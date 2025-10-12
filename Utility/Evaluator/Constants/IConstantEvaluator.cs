namespace Sapientia.Evaluators
{
	public interface IConstantEvaluator
	{
	}

	public interface IConstantEvaluator<T> : IConstantEvaluator
	{
		public ref readonly T Value { get; }
	}
}
