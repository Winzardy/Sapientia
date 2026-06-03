using System;

namespace Sapientia.Evaluators
{
	[Serializable]
	public sealed class ConstantEvaluator<TContext, TValue> : Evaluator<TContext, TValue>, IConstantEvaluator<TContext, TValue>
	{
		public TValue value;

		public Type ContextType { get => typeof(TContext); }

		public ref readonly TValue Value { get => ref value; }

		public ConstantEvaluator()
		{
		}

		public ConstantEvaluator(TValue value) => this.value = value;

		protected override TValue OnEvaluate(TContext context) => value;

		public static implicit operator TValue(ConstantEvaluator<TContext, TValue> evaluator)
			=> evaluator.value;

		public override string ToString() => value?.ToString() ?? string.Empty;
	}
}
