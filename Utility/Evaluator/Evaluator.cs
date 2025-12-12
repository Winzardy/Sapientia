#nullable disable
using System;

namespace Sapientia.Evaluators
{
	/// <summary>
	/// Представляет узел, который вычисляет значение на основе заданного контекста
	/// </summary>
	/// <typeparam name="TContext">Тип контекста, в котором выполняется вычисление</typeparam>
	/// <typeparam name="TValue">Тип вычисляемого значения</typeparam>
	/// <remarks>
	/// Для корневых узлов графа значений рекомендуется использовать <see cref="EvaluatedValue{TContext, TValue}"/>,
	/// так как он позволяет хранить дешевое константное значение, но внутри так же хранит <see cref="Evaluator{TContext, TValue}"/>
	/// </remarks>
	[Serializable]
	public abstract partial class Evaluator<TContext, TValue> : IEvaluator<TContext, TValue>
	{
		/// <summary>
		/// Вычисляет значение в зависимости от контекста
		/// </summary>
		/// <remarks>
		/// Повторный вызов запускает вычисление заново — внутри может использоваться рандом или логика, меняющая состояние.
		/// Чтобы переиспользовать полученное значение в логике, следует кешировать результат
		/// </remarks>
		public TValue Evaluate(TContext context) => OnEvaluate(context);

		protected abstract TValue OnEvaluate(TContext context);

		public static implicit operator Evaluator<TContext, TValue>(TValue value)
			=> new ConstantEvaluator<TContext, TValue>(value);

		public static implicit operator bool(Evaluator<TContext, TValue> evaluator) => evaluator != null;
	}
}
