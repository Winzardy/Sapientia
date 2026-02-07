#nullable disable
using System;

namespace Sapientia
{
	/// <summary>
	/// Перехватывает вызов на закрытие и в итоге сам должен вызвать у себя CloseRequest
	/// </summary>
	public interface ICloseInterceptor : ICloseRequestor
	{
		void RequestClose();
	}

	public interface ICloseRequestor
	{
		event Action CloseRequested;
	}
}
