using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_2022_3_OR_NEWER
using UnityEngine;
#endif

namespace Sapientia.Collections
{
	[Serializable]
	public class SerializableReferenceDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue, KeyReferenceValue<TKey, TValue>>
		where TValue : class
	{
		public SerializableReferenceDictionary() : base()
		{
		}

		public SerializableReferenceDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
		{
		}

		public SerializableReferenceDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary,
			comparer)
		{
		}
#if UNITY_2022_3_OR_NEWER
		public SerializableReferenceDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
		{
		}

		public SerializableReferenceDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(
			collection, comparer)
		{
		}
#endif
		public SerializableReferenceDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
		{
		}

		public SerializableReferenceDictionary(int capacity) : base(capacity)
		{
		}

		public SerializableReferenceDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
		{
		}
	}

	[Serializable]
	public class SerializableDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue, KeyValue<TKey, TValue>>
	{
		public SerializableDictionary() : base()
		{
		}

		public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
		{
		}

		public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
		{
		}
#if UNITY_2022_3_OR_NEWER
		public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
		{
		}

		public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(
			collection, comparer)
		{
		}
#endif
		public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
		{
		}

		public SerializableDictionary(int capacity) : base(capacity)
		{
		}

		public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
		{
		}
	}

	[Serializable]
	public abstract class SerializableDictionary<TKey, TValue, TKeyValue> : Dictionary<TKey, TValue>
#if UNITY_2022_3_OR_NEWER
		, ISerializationCallbackReceiver
#endif
		where TKeyValue : struct, IKeyValue<TKey, TValue>
	{
#if UNITY_2022_3_OR_NEWER
#if NEWTONSOFT
		[Newtonsoft.Json.JsonIgnore]
#endif
		[SerializeField, HideInInspector]
		protected TKeyValue[] elements;
#endif
		protected SerializableDictionary() : base()
		{
		}

		protected SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
		{
		}

		protected SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary,
			comparer)
		{
		}
#if UNITY_2022_3_OR_NEWER
		protected SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection)
		{
		}

		protected SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(
			collection, comparer)
		{
		}
#endif
		protected SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
		{
		}

		protected SerializableDictionary(int capacity) : base(capacity)
		{
		}

		protected SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
		{
		}

#if UNITY_2022_3_OR_NEWER
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (elements == null || elements.Length != Count)
				elements = new TKeyValue[Count];

			var i = 0;
			foreach (var pair in this)
			{
				var keyValue = default(TKeyValue);
				keyValue.Key = pair.Key;
				keyValue.Value = pair.Value;

				elements[i++] = keyValue;
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (elements == null)
				elements = new TKeyValue[Count];

			for (var i = 0; i < elements.Length; i++)
			{
#if UNITY_2022_3_OR_NEWER
				TryAdd(elements[i].Key, elements[i].Value);
#else
				if (ContainsKey(_newElement.Key))
					Add(_elements[i].Key, _elements[i].Value);
#endif
			}
		}
#endif
	}

	[Serializable]
	public struct KeyValue<TKey, TValue> : IKeyValue<TKey, TValue>
	{
		public TKey key;
		public TValue value;

		public TKey Key
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => key;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => key = value;
		}

		public TValue Value
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this.value = value;
		}

		public static implicit operator (TKey, TValue)(KeyValue<TKey, TValue> keyValue)
		{
			return (keyValue.Key, keyValue.Value);
		}
	}

	[Serializable]
	public struct KeyReferenceValue<TKey, TValue> : IKeyValue<TKey, TValue>
		where TValue : class
	{
		public TKey key;

#if UNITY_2022_3_OR_NEWER
		[SerializeReference]
#endif
		public TValue value;

		public TKey Key
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => key;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => key = value;
		}

		public TValue Value
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this.value = value;
		}
	}

	public interface IKeyValue<TKey, TValue>
	{
		public TKey Key { get; set; }
		public TValue Value { get; set; }
	}
}
