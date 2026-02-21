using System;
using Sapientia;
using static SharedLogic.TimeSharedNode;

namespace SharedLogic
{
	public partial class TimeSharedNode : SharedNode<SaveData>
	{
		private readonly IDateTimeProvider _timeProvider;

		private TimeSpan _dateTimeOffset;
		private DateTime _dateTime;

		/// <summary>
		/// Время без учета смещения!
		/// </summary>
		public DateTime DateTimeWithoutOffset
		{
			get
			{
#if CLIENT
				return GetDateTime();
#else
				return _dateTime;
#endif
			}
		}

		public DateTime DateTime => DateTimeWithoutOffset + _dateTimeOffset;

		public TimeSpan DateTimeOffset => _dateTimeOffset;

		public TimeSharedNode(IDateTimeProvider timeProvider)
		{
			_timeProvider = timeProvider;
			_dateTime = _timeProvider.DateTimeWithoutOffset;
		}

		/// <summary>
		/// Увеличивает смещение относительно серверного времени
		/// </summary>
		internal void AddTimeOffset(TimeSpan offset)
		{
			SetTimeOffset(_dateTimeOffset + offset);
		}

		/// <summary>
		/// Увеличивает смещение относительно серверного времени
		/// </summary>
		internal void SetTimeOffset(TimeSpan offset)
		{
			_dateTimeOffset = offset;
			SLDebug.Log($"Update dateTime, new: {DateTime} (real: {DateTimeWithoutOffset}, offset: {_dateTimeOffset})");
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
				error = new(TimeSetError.Code.TimestampLessThanCurrent)
				{
					prevTimestamp = _dateTime.Ticks,
					nextTimestamp = timestamp
				};
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
			_dateTime = data.timestamp != 0 ? new DateTime(data.timestamp, DateTimeKind.Utc) : _timeProvider.DateTimeWithoutOffset;
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
		public static long GetTimestampWithoutOffset(this ISharedRoot root)
			=> GetDateTimeWithoutOffset(root).Ticks;

		public static long GetTimestamp(this ISharedRoot root)
			=> GetDateTime(root).Ticks;

		public static DateTime GetDateTimeWithoutOffset(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
				.DateTimeWithoutOffset;

		public static DateTime GetDateTime(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
				.DateTime;

		public static TimeSpan GetDateTimeOffset(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
				.DateTimeOffset;

		public static bool IsTimedCommand<T>(in T command) where T : struct, ICommand
		{
			return IsTimedCommand(typeof(T));
		}

		public static bool IsTimedCommand(Type commandType)
		{
			return commandType == typeof(TimeSetCommand);
		}
	}

	public struct TimeSetError
	{
		public enum Code
		{
			None,

			TimestampLessThanCurrent
		}

		public readonly Code code;

		public long prevTimestamp;
		public long nextTimestamp;

		public TimeSetError(Code code) : this()
		{
			this.code = code;
		}

		public static implicit operator TimeSetError(Code code) => new(code);

		public override string ToString() =>
			$"Can't set timestamp with code [ {code} ] (prev: {prevTimestamp}, -> next: {nextTimestamp})";
	}
}
