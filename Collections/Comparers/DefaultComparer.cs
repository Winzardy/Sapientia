using System;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public struct DefaultComparer<T> : IComparer<T> where T : IComparable<T>
	{
		/// <summary>
		/// Compares two values.
		/// </summary>
		/// <param name="x">First value to compare.</param>
		/// <param name="y">Second value to compare.</param>
		/// <returns>A signed integer that denotes the relative values of `x` and `y`:
		/// 0 if they're equal, negative if `x &lt; y`, and positive if `x &gt; y`.</returns>
		public int Compare(T x, T y) => x.CompareTo(y);
	}
}
