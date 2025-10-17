using System;

namespace Sapientia
{
	public interface IDateTimeProvider
	{
		public DateTime DateTimeWithoutOffset { get; }
	}

	public interface IDateTimeProviderWithVirtual : IDateTimeProvider
	{
		/// <summary>
		/// Время, которое могут двигать
		/// </summary>
		public DateTime DateTime { get; }
	}
}
