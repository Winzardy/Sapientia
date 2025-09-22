using System;
using Sapientia;
using static SharedLogic.TimeSharedNode;

namespace SharedLogic
{
	public class TimeSharedNode : SharedNode<SaveData>
	{
		private TimeSpan _dateTimeOffset;
		private DateTime _dateTime;
		private IDateTimeProvider _timeProvider;

		public DateTime DateTime => _dateTime;
		public DateTime VirtualDateTime => DateTime + _dateTimeOffset;

		public TimeSharedNode(IDateTimeProvider timeProvider)
		{
			_timeProvider = timeProvider;

			_dateTime = _timeProvider.DateTime;
		}

		/// <summary>
		/// Увеличивает смещение относительно серверного времени
		/// </summary>
		internal void AddTimeOffset(TimeSpan offset)
		{
			_dateTimeOffset += offset;
		}

		/// <summary>
		/// Очистить смещение относительного серверного времени
		/// </summary>
		internal void ClearTimeOffset()
		{
			_dateTimeOffset = TimeSpan.Zero;
		}

		public bool CanSetTimestamp(long timestamp, out TimeSetError? error)
		{
			if (_dateTime.Ticks > timestamp)
			{
				error = TimeSetError.Code.TimestampLessThanCurrent;
				return false;
			}

			error = null;
			return true;
		}

		internal void SetTimestamp(long timestamp)
		{
			_dateTime = new DateTime(timestamp);
		}

		protected override void OnLoad(in SaveData data)
		{
			_dateTimeOffset = new TimeSpan(data.timestampOffset);
			_dateTime = data.timestamp == 0 ? new DateTime(data.timestamp) : _timeProvider.DateTime;
		}

		protected override void OnSave(out SaveData data)
		{
			data.timestampOffset = _dateTimeOffset.Ticks;
			data.timestamp = _dateTime.Ticks;
		}

		[Serializable]
		public struct SaveData
		{
			public long timestamp;
			public long timestampOffset;
		}
	}

	public static class SharedTimeUtility
	{
		public static long GetTimestamp(this ISharedRoot root)
			=> GetDateTime(root).Ticks;

		public static long GetVirtualTimestamp(this ISharedRoot root)
			=> GetVirtualDateTime(root).Ticks;

		public static DateTime GetDateTime(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
			   .DateTime;

		public static DateTime GetVirtualDateTime(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
			   .VirtualDateTime;
	}

	public struct TimeSetError
	{
		public enum Code
		{
			None,

			TimestampLessThanCurrent
		}

		public readonly Code code;

		public TimeSetError(Code code) : this()
		{
			this.code = code;
		}

		public static implicit operator TimeSetError(Code code) => new(code);

		public override string ToString() => $"Can't set timestamp with code [ {code} ]";
	}
}
