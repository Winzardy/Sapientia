using System;

namespace Sapientia
{
	public interface ISubscriptionToken : IDisposable
	{
		int Generation { get; }
	}
}
