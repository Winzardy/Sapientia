using System;

namespace Sapientia.Collections
{
	public static class SpanExt
	{
		public static void RemoveSwapBack<T>(this ref Span<T> span, int index)
		{
			(span[index], span[^1]) = (span[^1], span[index]);
			span = span[..^1];
		}
	}
}
