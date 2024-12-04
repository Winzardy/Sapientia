using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;
using Sapientia.ServiceManagement;

namespace Sapientia.Collections.Archetypes
{
	public struct EmptyValue {}

	public struct OptionalValue<TValue>
	{
		private byte _isEnabled;
		public TValue value;

		private OptionalValue(TValue value)
		{
			_isEnabled = 1;
			this.value = value;
		}

		public void SetEnabled(bool isEnabled = true)
		{
			_isEnabled = (byte)(isEnabled ? 1 : 0);
		}

		public readonly bool IsEnabled()
		{
			return _isEnabled == 1;
		}

		public readonly bool TryGetValue(out TValue value)
		{
			if (_isEnabled == 1)
			{
				value = this.value;
				return true;
			}
			value = default;
			return false;
		}

		public static implicit operator TValue(OptionalValue<TValue> value)
		{
			return value._isEnabled == 1 ? value.value : default;
		}

		public static implicit operator OptionalValue<TValue>(TValue value)
		{
			return new OptionalValue<TValue>(value);
		}
	}

	public readonly struct OneShotValue<TValue>
	{
		private readonly uint _version;
		private readonly TValue _value;

		private OneShotValue(TValue value)
		{
			_version = World.Instance.Tick;
			_value = value;
		}

		public readonly bool IsValid()
		{
			return _version == World.Instance.Tick;
		}

		public readonly bool TryGetValue(out TValue value)
		{
			if (IsValid())
			{
				value = _value;
				return true;
			}
			value = default;
			return false;
		}

		public static implicit operator TValue(OneShotValue<TValue> value)
		{
			return value.IsValid() ? value._value : default;
		}

		public static implicit operator OneShotValue<TValue>(TValue value)
		{
			return new OneShotValue<TValue>(value);
		}
	}

	public struct ArchetypeElement<TValue>
	{
		public readonly Entity entity;
		public TValue value;

		public ArchetypeElement(Entity entity, TValue value)
		{
			this.entity = entity;
			this.value = value;
		}
	}

	public class Archetype : Archetype<EmptyValue>
	{
		public Archetype(SparseSet<ArchetypeElement<EmptyValue>>.ResetAction resetAction = null) : base(resetAction)
		{
		}

		public Archetype(int elementsCount, SparseSet<ArchetypeElement<EmptyValue>>.ResetAction resetAction = null) : base(elementsCount, resetAction)
		{
		}
	}

	public class Archetype<TValue> : BaseArchetype, IEnumerable<ArchetypeElement<TValue>>
	{
		public delegate void EntityDestroyAction(ref ArchetypeElement<TValue> element);
		public delegate void EntityArrayDestroyAction(in ArchetypeElement<TValue>[] elements, int count);

		public struct DestroyEvents
		{
			public EntityDestroyAction OnEntityDestroyEvent;
			public EntityArrayDestroyAction OnEntityArrayDestroyEvent;
		}

		public static Archetype<TValue> Instance
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ServiceLocator<Archetype<TValue>>.Instance;
		}

		private static readonly TValue DEFAULT = default;

		private readonly SparseSet<ArchetypeElement<TValue>> _elements;
		private event EntityDestroyAction OnEntityDestroyEvent;
		private event EntityArrayDestroyAction OnEntityArrayDestroyEvent;

		public ref readonly ArchetypeElement<TValue>[] Elements => ref _elements.GetValueArray();
		public int Count => _elements.Count;

		public Archetype(SparseSet<ArchetypeElement<TValue>>.ResetAction resetAction = null,
			DestroyEvents? destroyEvents = null) : this(ServiceLocator<EntitiesState>.Instance.EntitiesCapacity, resetAction, destroyEvents)
		{

		}

		public Archetype(int elementsCount, SparseSet<ArchetypeElement<TValue>>.ResetAction resetAction = null,
			DestroyEvents? destroyEvents = null) : this(elementsCount, ServiceLocator<EntitiesState>.Instance.EntitiesCapacity, resetAction, destroyEvents)
		{

		}

		internal Archetype(int elementsCount, int entitiesCapacity, SparseSet<ArchetypeElement<TValue>>.ResetAction resetAction = null, DestroyEvents? destroyEvents = null)
		{
			_elements = new SparseSet<ArchetypeElement<TValue>>(elementsCount, entitiesCapacity, resetAction);

			if (destroyEvents == null)
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent += RemoveSwapBackElement;
			else
			{
				OnEntityDestroyEvent = destroyEvents.Value.OnEntityDestroyEvent;
				OnEntityArrayDestroyEvent = destroyEvents.Value.OnEntityArrayDestroyEvent;

				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent += OnEntityDestroy;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly TValue ReadElement(Entity entity)
		{
			if (!_elements.Has(entity.id))
				return ref DEFAULT;
			return ref _elements.Get(entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly TValue ReadElementNoCheck(Entity entity)
		{
			return ref _elements.Get(entity.id).value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleList<TValue> ReadElements(SimpleList<Entity> entities)
		{
			var result = new SimpleList<TValue>(entities.Count);
			foreach (var entity in entities)
			{
				if (_elements.Has(entity.id))
					result.Add(_elements.Get(entity.id).value);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasElement(Entity entity)
		{
			return _elements.Has(entity.id);
		}

		public ref TValue GetElement(Entity entity)
		{
			if (_elements.Has(entity.id))
			{
				ref var element = ref _elements.Get(entity.id);
				System.Diagnostics.Debug.Assert(element.entity == entity);
				return ref element.value;
			}
			else
			{
#if UNITY_EDITOR || (UNITY_5_3_OR_NEWER && ARCHETYPES_DEBUG)
				var oldCapacity = _elements.Capacity;
				ref var element = ref _elements.EnsureGet(entity.id);
				if (oldCapacity != _elements.Capacity)
					UnityEngine.Debug.LogWarning($"Archetype of {typeof(TValue).Name} was expanded. Count: {_elements.Count - 1}->{_elements.Count}; Capacity: {oldCapacity}->{_elements.Capacity}");
#else
				ref var element = ref _elements.EnsureGet(entity.id);
#endif
				element = new ArchetypeElement<TValue>(entity, default);
				return ref element.value;
			}
		}

		public void Clear()
		{
			ref readonly var valueArray = ref _elements.GetValueArray();

			if (OnEntityDestroyEvent != null)
			{
				for (var i = 0; i < _elements.Count; i++)
				{
					OnEntityDestroyEvent.Invoke(ref _elements.Get(valueArray[i].entity.id));
				}
			}
			for (var i = 0; i < _elements.Count; i++)
			{
				_elements.Get(valueArray[i].entity.id) = default;
			}

			_elements.ClearFast();
		}

		public void ClearFast()
		{
			if (OnEntityDestroyEvent != null)
			{
				OnEntityArrayDestroyEvent!.Invoke(_elements.GetValueArray(), _elements.Count);
			}

			_elements.ClearFast();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSwapBackElement(Entity entity)
		{
			_elements.RemoveSwapBack(entity.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRemoveSwapBackElement(Entity entity, out ArchetypeElement<TValue> value)
		{
			return _elements.TryRemoveSwapBack(entity.id, out value);
		}

		private void OnEntityDestroy(Entity entity)
		{
			if (!_elements.Has(entity.id))
				return;

			OnEntityDestroyEvent!.Invoke(ref _elements.Get(entity.id));

			if (!_elements.Has(entity.id))
				return;
			_elements.RemoveSwapBack(entity.id);
		}

		~Archetype()
		{
			if (ServiceLocator<EntitiesState>.Instance == null)
				return;

			if (OnEntityDestroyEvent == null)
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent -= RemoveSwapBackElement;
			else
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent -= OnEntityDestroy;
		}

		public IEnumerator<ArchetypeElement<TValue>> GetEnumerator()
		{
			return _elements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public abstract class BaseArchetype
	{

	}
}
