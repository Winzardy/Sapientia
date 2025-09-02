using System;

namespace Sapientia
{
	public interface IDateTimeProvider
	{
		public DateTime GetTime(bool useSimulateTime = false);
	}
}
