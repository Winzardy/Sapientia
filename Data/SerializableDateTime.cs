using System;
using System.Globalization;

namespace Survivor.Interop
{
	/// <remarks>
	/// В UTC
	/// </remarks>
	[Serializable]
	public struct SerializableDateTime
	{
		public static SerializableDateTime Empty = new(0);

		public long ticks;

		public bool IsEmpty { get => ticks == 0; }

		public static SerializableDateTime Now { get => DateTime.UtcNow; }

		public SerializableDateTime(long ticks)
		{
			this.ticks = ticks;
		}

		public SerializableDateTime(DateTime dateTime) : this(
			dateTime.Kind == DateTimeKind.Local
				? dateTime.ToUniversalTime().Ticks
				: dateTime.Ticks)
		{
		}

		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture);
		}

		public string ToString(CultureInfo info)
		{
			DateTime dateTime = this;
			return dateTime.ToString(info);
		}

		public static implicit operator DateTime(SerializableDateTime serialized) => new(serialized.ticks, DateTimeKind.Utc);
		public static implicit operator SerializableDateTime(DateTime dateTime) => new(dateTime);
		public static implicit operator SerializableDateTime(long ticks) => new(ticks);
	}
}
