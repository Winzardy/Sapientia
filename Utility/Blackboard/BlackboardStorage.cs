using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.Pooling;

namespace Sapientia
{
	public interface IBlackboardStorage
	{
		Type StorageType { get; }

		internal void ReleaseToPool();
	}

	public interface IBlackboardStorage<T> : IBlackboardStorage
	{
		T Get(string? key);
		bool Contains(string? key);
		void Overwrite(in T value, string? key = null);
		BlackboardToken<T> Register(in T value, string? key);
		void Unregister(BlackboardToken<T> token);
	}

	internal class BlackboardStorage<T> : IBlackboardStorage<T>, IPoolable
	{
		private Blackboard? _blackboard;

		private ValueEntry? _single;
		private Dictionary<string, ValueEntry> _keyToValueEntry;

		public Type StorageType { get => typeof(T); }

		public Blackboard? Blackboard { get => _blackboard; }

		public BlackboardStorage()
		{
			_keyToValueEntry = new Dictionary<string, ValueEntry>();
		}

		void IPoolable.Release()
		{
			_blackboard = null;
			_keyToValueEntry.Clear();
		}

		internal void Bind(Blackboard blackboard)
		{
			_blackboard = blackboard;
		}

		public bool Contains(string? key = null) => TryGetValueEntry(key, out _);

		public T Get(string? key = null)
		{
			if (!TryGetValueEntry(key, out var entry))
			{
				var msg = $"{_blackboard!.GetName()}: ${typeof(T)} not found" +
					(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
				throw new BlackboardException(msg);
			}

			return entry!.value;
		}

		public BlackboardToken<T> Register(in T value, string? key = null)
		{
			if (Contains(key))
			{
				var msg = $"{_blackboard!.GetName()}: {typeof(T)} already registered" +
					(!key.IsNullOrEmpty() ? $" with key [ {key} ]" : "");
				throw new BlackboardException(msg);
			}

			var newEntry = Pool<ValueEntry>.Get();
			{
				newEntry.value = value;
			}
			if (key == null)
			{
				_single = newEntry;
			}
			else
			{
				_keyToValueEntry[key] = newEntry;
			}

			var token = Pool<BlackboardToken<T>>.Get();
			token.Bind(this, key);
			return token;
		}

		public void Unregister(BlackboardToken<T> token)
		{
			ValueEntry? entry;
			if (token.Key == null)
			{
				entry   = _single;
				_single = null;
			}
			else
			{
				if (!_keyToValueEntry.Remove(token.Key, out entry))
				{
					var blackboard = token.Blackboard;
					var key = token.Key;
					var msg = $"{blackboard.GetName()}: {typeof(T)} not registered" +
						(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
					throw new BlackboardException(msg);
				}
			}

			Pool<ValueEntry>.Release(entry!);
			Pool<BlackboardToken<T>>.Release(token);
		}

		public void Overwrite(in T value, string? key = null)
		{
			if (!TryGetValueEntry(key, out var entry))
			{
				var msg = $"{_blackboard!.GetName()}: {typeof(T)} not registered" +
					(!key.IsNullOrEmpty() ? $" with key [ {key} ]" : "");
				throw new BlackboardException(msg);
			}

			entry!.value = value;
		}

		private bool TryGetValueEntry(string? key, out ValueEntry? entry)
		{
			if (key == null)
			{
				entry = _single;
				return _single != null;
			}

			return _keyToValueEntry.TryGetValue(key, out entry);
		}

		void IBlackboardStorage.ReleaseToPool() => Pool<BlackboardStorage<T>>.Release(this);

		internal class ValueEntry : IPoolable
		{
			internal T value;

			void IPoolable.Release()
				=> value = default;
		}
	}
}
