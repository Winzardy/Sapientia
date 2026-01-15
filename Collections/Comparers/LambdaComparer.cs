using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public struct LambdaComparer<T> : IComparer<T>
	{
		private Comparison<T> _comparison;

		public LambdaComparer(Comparison<T> comparison)
		{
			_comparison = comparison;
		}

		public int Compare(T x, T y) => _comparison.Invoke(x, y);
	}
}
