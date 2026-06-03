using System.Linq;

namespace Sapientia.Utility
{
	public static class UrlUtility
	{
		public static string ConcatUrl(params string[]? parts)
		{
			if (parts == null || parts.Length == 0)
				return string.Empty;

			var normalizedParts = parts
				.Where(p => !string.IsNullOrWhiteSpace(p))
				.Select(p => p.Trim(' ', '/'))
				.Where(p => !string.IsNullOrEmpty(p));

			return string.Join("/", normalizedParts);
		}
	}
}
