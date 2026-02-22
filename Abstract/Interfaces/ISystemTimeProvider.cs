using System;

namespace Sapientia
{
	public interface ISystemTimeProvider
	{
		/// <summary>
		/// Реальное системное время (без смещения)
		/// </summary>
		public DateTime SystemTime { get; }
	}

	public interface IVirtualTimeProvider : ISystemTimeProvider
	{
		/// <summary>
		/// Игровое время (с учетом смещения)
		/// </summary>
		public DateTime DateTime { get; }

		/// <summary>
		/// Смещение по времени
		/// </summary>
		public TimeSpan Offset { get; }
	}
}
