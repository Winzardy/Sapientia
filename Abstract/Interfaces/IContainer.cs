using System;

namespace Sapientia
{
	/// <summary>
	/// Технический интерфейс, используемый инспектором для определения типа значения
	/// </summary>
	/// <remarks>Утилитарный интерфейс, предназначенный исключительно для нужд инспектора</remarks>
	public interface IContainer
	{
		public Type ValueType { get; }
	}

	/// <inheritdoc/>
	public interface IContainer<T> : IContainer
	{
		Type IContainer.ValueType => typeof(T);
	}
}
