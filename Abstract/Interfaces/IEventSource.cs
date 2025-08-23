using System;

namespace Sapientia
{
	public interface IEventSource
	{
		public event Action Invoked;
	}
}
