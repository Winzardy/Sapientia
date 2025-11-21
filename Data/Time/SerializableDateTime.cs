using System;

namespace Sapientia.Data.Time
{
	/// <summary> UTC-independent serializable date time (stores ticks as long value) </summary>
	[Serializable]
	public struct SerializableDateTime
	{
#if UNITY_5_3_OR_NEWER
		[LongDateTime]
#endif
		public long ticks;

		public DateTime Value
		{
			get => new (ticks, DateTimeKind.Unspecified);
			set => ticks = value.Ticks;
		}
	}
}
