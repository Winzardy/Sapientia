using System;
using Sapientia;
using static SharedLogic.TimeSharedNode;

namespace SharedLogic
{
	public class TimeSharedNode : SharedNode<SaveData>
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
#if !CLIENT
				return _dateTime;
#else
				return GetDateTime();
#endif
			}
		}

		public DateTime DateTime => DateTimeWithoutOffset + _dateTimeOffset;

		public TimeSpan CurrentOffset => _dateTimeOffset;
#if CLIENT
		private bool _timeProviderSuppress;
#endif

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

#if CLIENT
		private DateTime GetDateTime()
		{
			if (_timeProvider == null || _timeProviderSuppress)
				return _dateTime;

			return _timeProvider.DateTimeWithoutOffset;
		}

		public TimeProviderSuppressFlow ProviderSuppressScope() => new(this);
		internal void SuppressTimeProvider(bool value) => _timeProviderSuppress = value;
#endif

		protected override void OnLoad(in SaveData data)
		{
			_dateTimeOffset = new TimeSpan(data.timestampOffset);
			_dateTime = data.timestamp == 0 ? new DateTime(data.timestamp) : _timeProvider.DateTimeWithoutOffset;
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

#if CLIENT
	public readonly ref struct TimeProviderSuppressFlow
	{
		private readonly TimeSharedNode _node;

		public TimeProviderSuppressFlow(TimeSharedNode node)
		{
			_node = node;
			_node.SuppressTimeProvider(true);
		}

		public void Dispose()
		{
			_node.SuppressTimeProvider(false);
		}
	}
#endif

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

		public static TimeSpan GetCurrentOffset(this ISharedRoot root)
			=> root.GetNode<TimeSharedNode>()
				.CurrentOffset;

		public static bool IsTimedCommand<T>(in T command) where T : struct, ICommand
		{
			return typeof(T) == typeof(TimeSetCommand);
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

		public TimeSetError(Code code) : this()
		{
			this.code = code;
		}

		public static implicit operator TimeSetError(Code code) => new(code);

		public override string ToString() => $"Can't set timestamp with code [ {code} ]";
	}
}
