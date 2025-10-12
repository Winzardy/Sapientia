using System;

namespace Sapientia.Evaluators
{
	public interface IRandomEvaluator
	{
	}

	public interface IRandomEvaluator<T> : IRandomEvaluator
		where T : struct, IComparable<T>
	{
	}
}
