using System;

namespace Sapientia
{
	public interface IHolder
	{
		public Type Type { get; }
	}

	public interface IHolder<T> : IHolder
	{
		Type IHolder.Type => typeof(T);
	}
}
