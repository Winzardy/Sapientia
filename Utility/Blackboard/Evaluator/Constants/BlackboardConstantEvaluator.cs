using System;
using Sapientia.Evaluator;

namespace Sapientia.BlackboardEvaluator
{
	[Serializable]
	public class BlackboardConstantEvaluator<T> : BlackboardEvaluator<T>, IConstantEvaluator<T>
	{
		public T value;

		public BlackboardConstantEvaluator(T value) => this.value = value;
		protected sealed override T OnGet(Blackboard blackboard) => value;
		public ref readonly T Value => ref value;

		public override string ToString(Blackboard blackboard) => value?.ToString() ?? string.Empty;
	}
}
