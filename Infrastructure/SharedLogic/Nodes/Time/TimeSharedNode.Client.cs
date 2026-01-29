#if CLIENT
using System;

namespace SharedLogic
{
	public partial class TimeSharedNode
	{
		private bool _timeProviderSuppress;

		public bool TimeProviderSuppress { get => _timeProviderSuppress; }

		private DateTime GetDateTime()
		{
			if (_timeProvider == null || _timeProviderSuppress)
				return _dateTime;

			return _timeProvider.DateTimeWithoutOffset;
		}

		public TimeProviderSuppressFlow ProviderSuppressScope() => new(this);
		internal void SuppressTimeProvider(bool value) => _timeProviderSuppress = value;
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
