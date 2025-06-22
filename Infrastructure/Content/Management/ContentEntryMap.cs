using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia;

namespace Content.Management
{
	public enum ContentEntryState
	{
		None, //or Cleared

		Building,
		Built
	}

	public static class ContentEntryMap
	{
		public static ContentEntryState State { get; private set; }

		public static Action Build;
		public static Action Clear;

		public static void SetState(ContentEntryState state) => State = state;
	}

	public static class ContentEntryMap<T>
	{
		private static bool _building;
		private static bool _clearing;

		private static readonly ContentDictionary<UniqueContentEntry<T>> _dictionary = new();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Register(UniqueContentEntry<T> entry)
		{
			if (!_building)
			{
				ContentEntryMap.Build += OnBuilt;
				_building = true;
			}

			if (!_dictionary.TryAdd(in entry.Guid, entry))
				throw ContentDebug.Exception($"Already registered entry of type: [ {typeof(T).Name} ] with guid: [ {entry.Guid} ]  ");

			if (entry.Value is IExternallyIdentifiable identifiable)
			{
				if (entry.ValueType.IsValueType)
				{
					ContentDebug.LogError($"Type [{entry.ValueType.FullName}] is a struct. Cannot assign Id via interface. Use class instead.");
				}
				else
				{
					identifiable.SetId(entry.Id);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Unregister(UniqueContentEntry<T> entry)
		{
			if (!_clearing)
			{
				ContentEntryMap.Clear += OnCleared;
				_clearing = true;
			}

			if (_dictionary.IsBuilding)
				_dictionary.Remove(in entry.Guid);
		}

		private static void OnBuilt()
		{
			ContentEntryMap.Build -= OnBuilt;

			_dictionary.Freeze();

			_building = false;
		}

		private static void OnCleared()
		{
			ContentEntryMap.Clear -= OnCleared;

			_dictionary.Clear();

			_clearing = false;
		}

		public static bool Any() => !_dictionary.IsEmpty;

		public static bool Contains(string id) => Any() && _dictionary.Contains(id);
		public static bool Contains(in SerializableGuid guid) => Any() && _dictionary.Contains(in guid);
		public static bool Contains(int index) => Any() && _dictionary.Contains(index);

		public static UniqueContentEntry<T> GetEntry(in SerializableGuid guid) => _dictionary[in guid];
		public static UniqueContentEntry<T> GetEntry(string id) => _dictionary[id];
		public static UniqueContentEntry<T> GetEntry(in int index) => _dictionary[index];

		public static ref readonly T Get(in SerializableGuid guid) => ref _dictionary[in guid].Value;
		public static ref readonly T Get(string id) => ref _dictionary[id].Value;
		public static ref readonly T Get(int index) => ref _dictionary[index].Value;

		public static string ToId(in SerializableGuid guid) => _dictionary[in guid].Id;
		public static string ToId(int index) => _dictionary[index].Id;

		public static ref readonly SerializableGuid ToGuid(string id) => ref _dictionary[id].Guid;
		public static ref readonly SerializableGuid ToGuid(int index) => ref _dictionary[index].Guid;

		public static int ToIndex(string id) => _dictionary[id].Index;
		public static int ToIndex(in SerializableGuid guid) => _dictionary[in guid].Index;

		public static IEnumerable<IUniqueContentEntry<T>> GetAll() => _dictionary.Values;
	}
}
