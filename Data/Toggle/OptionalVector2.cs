using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia
{
#if CLIENT
	[InlineProperty(LabelWidth = 14)]
#endif
	[Serializable]
	public struct OptionalVector2
	{
#if CLIENT
		[HorizontalGroup(0.5f)]
#endif
		public Toggle<float> x;
#if CLIENT
		[HorizontalGroup(0.5f)]
#endif
		public Toggle<float> y;

		public OptionalVector2(Toggle<float> x, Toggle<float> y)
		{
			this.x = x;
			this.y = y;
		}
	}
}
