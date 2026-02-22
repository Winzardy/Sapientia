using System;
using Sapientia;
using static SharedLogic.TimeSharedNode;

namespace SharedLogic
{
	public partial class TimeSharedNode : SharedNode<SaveData>, IVirtualTimeProvider
	{
		private const long TIMESTAMP_TOLERANCE_TICKS = TimeSpan.TicksPerMillisecond * 500;

		private readonly ISystemTimeProvider _timeProvider;

		private TimeSpan _dateTimeOffset;
		private DateTime _dateTime;

		/// <summary>
		/// Реальное системное время (без игрового смещения)
		/// </summary>
		public DateTime SystemTime
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

		/// <summary>
		/// Игровое время (с учетом игрового смещения)
		/// </summary>
		public DateTime DateTime { get => SystemTime + Offset; }

		/// <summary>
		/// Игровое смещение
		/// </summary>
		public TimeSpan Offset { get => _dateTimeOffset; }

		public TimeSharedNode(ISystemTimeProvider timeProvider)
		{
			_timeProvider = timeProvider;
			_dateTime = _timeProvider.SystemTime;
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
			SLDebug.Log($"Update dateTime, new: {DateTime} (real: {SystemTime}, offset: {_dateTimeOffset})");
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
			var currentTimestamp = _dateTime.Ticks;

			if (currentTimestamp > timestamp)
			{
				var delta = currentTimestamp - timestamp;
				if (delta > TIMESTAMP_TOLERANCE_TICKS)
				{
					error = new(TimeSetError.Code.TimestampLessThanCurrent)
					{
						prevTimestamp = currentTimestamp,
						nextTimestamp = timestamp
					};
					return false;
				}
			}

			error = null;
			return true;
		}

		/// <summary>
		/// Устанавливает timestamp, гарантируя монотонность.
		/// Если переданное значение меньше текущего, используется текущее
		/// </summary>
		internal void SetTimestampSafe(long timestamp)
		{
			var clamp = Math.Max(_dateTime.Ticks, timestamp);
			SetTimestamp(clamp);
		}

		internal void SetTimestamp(long timestamp)
		{
			_dateTime = new DateTime(timestamp, DateTimeKind.Utc);
		}

		protected override void OnLoad(in SaveData data)
		{
			_dateTimeOffset = new TimeSpan(data.timestampOffset);
			_dateTime = data.timestamp != 0
				? new DateTime(data.timestamp, DateTimeKind.Utc)
				: _timeProvider.SystemTime;
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
				.SystemTime;

		public static DateTime GetDateTime(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
				.DateTime;

		public static TimeSpan GetDateTimeOffset(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
				.Offset;

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
