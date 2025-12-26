using System;

namespace Sapientia
{
	/// <summary>
	/// Используется как контейнер данных заданного типа.
	/// Например, чтобы создать конфиг-прослойку (Смотри примеры в наследниках).
	/// </summary>
	public interface IDataContainer<T>
	{
		public T GetValue();
		public void SetValue(T value) => throw new NotImplementedException();
	}
}
