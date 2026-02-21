#if CLIENT
using System;

namespace SharedLogic
{
	public partial class TimeSharedNode
	{
		private int _timeProviderSuppressRequest;

		public bool TimeProviderSuppress { get => _timeProviderSuppressRequest > 0; }

		private DateTime GetDateTime()
		{
			if (_timeProvider == null || TimeProviderSuppress)
				return _dateTime;

			return _timeProvider.DateTimeWithoutOffset;
		}

		public TimeProviderSuppressFlow ProviderSuppressScope() => new(this);

		internal void SuppressTimeProvider(bool suppress)
		{
			if (suppress)
				_timeProviderSuppressRequest++;
			else
				_timeProviderSuppressRequest--;
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
}

#endif
