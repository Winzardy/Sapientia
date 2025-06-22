using System;
using System.Collections;
using System.Collections.Generic;

namespace Sapientia.Collections
{
	public abstract class Registry<T> : IEnumerable<T>
	{
		protected HashSet<T> _registry = new();

		public event Action<T> Registered;
		public event Action<T> Unregistered;

		public bool Register(T value)
		{
			if (!_registry.Add(value))
				return false;

			Registered?.Invoke(value);
			return true;
		}

		public bool Unregister(T value)
		{
			if (!_registry.Remove(value))
				return false;

			Unregistered?.Invoke(value);
			return true;
		}

		public void Subscribe(Action<T> onRegistered, Action<T> onUnregistered, bool invokeForAlreadyRegistered = true)
		{
			if (invokeForAlreadyRegistered)
				foreach (var health in _registry)
					onRegistered(health);

			Registered += onRegistered;
			Unregistered += onUnregistered;
		}

		public void Unsubscribe(Action<T> onRegistered, Action<T> onUnregistered)
		{
			Registered -= onRegistered;
			Unregistered -= onUnregistered;
		}

		public IEnumerator<T> GetEnumerator() => _registry.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _registry.GetEnumerator();

		public IEnumerable<T> GetAll() => _registry;
		public bool IsEmpty() => _registry.IsEmpty();
		public bool Any() => !IsEmpty();
	}
}
