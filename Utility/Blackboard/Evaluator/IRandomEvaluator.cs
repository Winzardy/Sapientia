using System;

namespace Sapientia.Evaluator
{
	public interface IRandomEvaluator
	{
	}

	public interface IRandomEvaluator<T> : IRandomEvaluator
		where T : struct, IComparable<T>
	{
	}
}
