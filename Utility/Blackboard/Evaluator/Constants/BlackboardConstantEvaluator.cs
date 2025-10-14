using System;

namespace Sapientia.Evaluator.Blackboard
{
	[Serializable]
	public class BlackboardConstantEvaluator<T> : BlackboardEvaluator<T>, IConstantEvaluator<T>
	{
		public T value;

		public BlackboardConstantEvaluator(T value) => this.value = value;
		protected sealed override T OnGet(Sapientia.Blackboard blackboard) => value;
		public ref readonly T Value => ref value;

		public override string ToString() => value?.ToString() ?? string.Empty;
	}
}
