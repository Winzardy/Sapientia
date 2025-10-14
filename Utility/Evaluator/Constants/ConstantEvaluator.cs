using System;

namespace Sapientia.Evaluators
{
	[Serializable]
	public sealed class ConstantEvaluator<TContext, TValue> : Evaluator<TContext, TValue>, IConstantEvaluator<TValue>
	{
		public TValue value;

		public ConstantEvaluator()
		{
		}

		public ConstantEvaluator(TValue value) => this.value = value;

		protected override TValue OnGet(TContext context) => value;

		public ref readonly TValue Value => ref value;

		public static implicit operator TValue(ConstantEvaluator<TContext, TValue> evaluator)
			=> evaluator.value;

		public override string ToString()
		{
			//TODO: костыль убрать
			if (value is int intValue)
				return intValue > 1 ? intValue.ToString() : string.Empty;

			return value?.ToString() ?? string.Empty;
		}
	}
}
