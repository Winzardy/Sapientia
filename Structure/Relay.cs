using System;

namespace Sapientia
{
	public abstract class Relay<T> : IDisposable
	{
		private T _source;

		public void Bind(T source)
		{
			if (_source != null)
				OnClear(_source);

			_source = source;
			OnBind(source);
		}

		public void Dispose() => Clear();

		public void Clear()
		{
			if (_source == null)
				return;

			OnClear(_source);
			_source = default;
		}

		protected abstract void OnBind(T source);

		protected abstract void OnClear(T source);
	}
}
