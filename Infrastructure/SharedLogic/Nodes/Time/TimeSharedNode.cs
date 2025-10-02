using System;
using Sapientia;
using static SharedLogic.TimeSharedNode;

namespace SharedLogic
{
	public class TimeSharedNode : SharedNode<SaveData>
	{
		private readonly IDateTimeProvider _timeProvider;

		private TimeSpan _dateTimeOffset;
		private DateTime _realDateTime;

		/// <summary>
		/// Время без учета смещения!
		/// </summary>
		public DateTime DateTimeWithoutOffset
		{
			get
			{
#if !CLIENT
				return _realDateTime;
#else
				return GetDateTime();
#endif
			}
		}

		public DateTime DateTime => DateTimeWithoutOffset + _dateTimeOffset;

		private bool _timeProviderSuppress;

		public TimeSharedNode(IDateTimeProvider timeProvider)
		{
			_timeProvider = timeProvider;
			_realDateTime = _timeProvider.DateTime;
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
			if (_realDateTime.Ticks > timestamp)
			{
				error = TimeSetError.Code.TimestampLessThanCurrent;
				return false;
			}

			error = null;
			return true;
		}

		internal void SetTimestamp(long timestamp)
		{
			_realDateTime = new DateTime(timestamp);
		}

#if CLIENT
		private DateTime GetDateTime()
		{
			if (_timeProvider == null || _timeProviderSuppress)
				return _realDateTime;

			return _timeProvider.DateTime;
		}

		internal TimeProviderSuppressFlow ProviderSuppressFlow() => new(this);
		internal void SuppressTimeProvider(bool value) => _timeProviderSuppress = value;
#endif
		protected override void OnLoad(in SaveData data)
		{
			_dateTimeOffset = new TimeSpan(data.timestampOffset);
			_realDateTime = data.timestamp == 0 ? new DateTime(data.timestamp) : _timeProvider.DateTime;
		}

		protected override void OnSave(out SaveData data)
		{
			data.timestampOffset = _dateTimeOffset.Ticks;
			data.timestamp = _realDateTime.Ticks;
		}

		[Serializable]
		public struct SaveData
		{
			public long timestamp;
			public long timestampOffset;
		}
	}

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
