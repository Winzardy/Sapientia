using System;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Sapientia
{
#if CLIENT
	[InlineProperty(LabelWidth = 28)]
#endif
	[Serializable]
	public struct OptionalRange<T>
	{
#if CLIENT
		[HorizontalGroup(0.5f)]
#endif
		public Toggle<T> min;
#if CLIENT
		[HorizontalGroup(0.5f)]
#endif
		public Toggle<T> max;

		public OptionalRange(Toggle<T> min, Toggle<T> y)
		{
			this.min = min;
			this.max = y;
		}
	}
}
