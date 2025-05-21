#if CLIENT
using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Sapientia
{
	[Conditional("UNITY_EDITOR")]
	public class MinimumParentAttribute : ParentAttribute
	{
		/// <summary>
		/// The minimum value for the property.
		/// </summary>
		public double MinValue;

		/// <summary>
		/// The string with which to resolve a minimum value. This could be a field, property or method name, or an expression.
		/// </summary>
		public string Expression;

		/// <summary>
		/// Sets a minimum value for the property in the inspector.
		/// </summary>
		/// <param name="minValue">The minimum value.</param>
		public MinimumParentAttribute(double minValue)
		{
			this.MinValue = minValue;
		}

		/// <summary>
		/// Sets a minimum value for the property in the inspector.
		/// </summary>
		/// <param name="expression">The string with which to resolve a minimum value. This could be a field, property or method name, or an expression.</param>
		public MinimumParentAttribute(string expression)
		{
			this.Expression = expression;
		}

		public override Attribute Convert()
			=> new MinValueAttribute(MinValue)
			{
				Expression = Expression
			};
	}
}
#endif
