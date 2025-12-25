using System;

namespace Sapientia.Evaluators
{
	public interface IConstantEvaluator
	{
		public Type ValueType { get; }
		public Type ContextType { get; }
	}

	public interface IConstantEvaluator<T> : IConstantEvaluator
	{
		public ref readonly T Value { get; }
		Type IConstantEvaluator.ValueType { get => typeof(T); }
	}

	public interface IConstantEvaluator<in TContext,T>  : IConstantEvaluator<T>
	{
		Type IConstantEvaluator.ContextType { get => typeof(TContext); }
	}

	public class DisableConstantEvaluatorAttribute : Attribute
	{
	}
}
