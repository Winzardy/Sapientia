using System;
using Sapientia.Deterministic;
using Sapientia.Extensions;

namespace Sapientia.Comparison
{
	public static class ComparisonOperatorUtility
	{
		public static bool Compare(this int a, ComparisonOperator @operator, int b)
		{
			return @operator switch
			{
				ComparisonOperator.GreaterOrEqual => a >= b,
				ComparisonOperator.LessOrEqual => a <= b,
				ComparisonOperator.Greater => a > b,
				ComparisonOperator.Less => a < b,
				ComparisonOperator.Equal => a == b,
				ComparisonOperator.NotEqual => a != b,
				_ => throw new NotImplementedException(),
			};
		}

		public static bool Compare(this float a, ComparisonOperator @operator, float b)
		{
			return @operator switch
			{
				ComparisonOperator.GreaterOrEqual => a.IsApproximatelyEqual(b) || a >= b,
				ComparisonOperator.LessOrEqual => a.IsApproximatelyEqual(b) || a <= b,
				ComparisonOperator.Greater => a > b,
				ComparisonOperator.Less => a < b,
				ComparisonOperator.Equal => a.IsApproximatelyEqual(b),
				ComparisonOperator.NotEqual => !a.IsApproximatelyEqual(b),
				_ => throw new NotImplementedException(),
			};
		}

		public static bool Compare(this Fix64 a, ComparisonOperator @operator, Fix64 b)
		{
			return @operator switch
			{
				ComparisonOperator.GreaterOrEqual => a >= b,
				ComparisonOperator.LessOrEqual => a <= b,
				ComparisonOperator.Greater => a > b,
				ComparisonOperator.Less => a < b,
				ComparisonOperator.Equal => a == b,
				ComparisonOperator.NotEqual => a != b,
				_ => throw new NotImplementedException(),
			};
		}
	}
}
