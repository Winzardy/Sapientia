#if CLIENT
using UnityEngine;

namespace Sapientia.Extensions
{
	public static class UnityColorUtility
	{
		public static Color WithAlpha(this Color color, in float alpha)
		{
			color.a = alpha;
			return color;
		}
	}
}
#endif
