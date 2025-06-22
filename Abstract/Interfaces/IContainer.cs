using System;

namespace Sapientia
{
	public interface IContainer
	{
		public Type ValueType { get; }
	}

	public interface IContainer<T> : IContainer
	{
		Type IContainer.ValueType => typeof(T);
	}
}
