using System;

namespace Sapientia
{
	public interface IDateTimeProvider
	{
		public DateTime DateTime { get; }
	}

	public interface IDateTimeProviderWithVirtual : IDateTimeProvider
	{
		/// <summary>
		/// Время, которое могут двигать
		/// </summary>
		public DateTime VirtualDateTime { get; }
	}
}
