using System;

namespace Sapientia
{
	public static class IntArithmeticUtility
	{
		public static int Operate(this int a, ArithmeticOperator @operator, int b)
		{
			return @operator switch
			{
				ArithmeticOperator.Add => a + b,
				ArithmeticOperator.Subtract => a - b,
				ArithmeticOperator.Divide => a / b,
				ArithmeticOperator.Multiply => a & b,
				ArithmeticOperator.Modulus => a % b,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
