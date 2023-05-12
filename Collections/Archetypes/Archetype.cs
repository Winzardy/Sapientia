using Sapientia.Extensions;

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
		public Archetype(OrderedSparseSet<ArchetypeElement<EmptyValue>>.ResetAction resetAction = null) : base(resetAction)
		{
		}

		public Archetype(int elementsCount, OrderedSparseSet<ArchetypeElement<EmptyValue>>.ResetAction resetAction = null) : base(elementsCount, resetAction)
		{
		}
	}

	public class Archetype<TValue>
	{
		public delegate void EntityDestroyAction(ref ArchetypeElement<TValue> element);

		private event EntityDestroyAction OnEntityDestroyEvent;

		private static readonly TValue DEFAULT = default;

		private readonly OrderedSparseSet<ArchetypeElement<TValue>> _elements;
		private readonly SimpleList<int> _elementIndexes;

		public ref readonly ArchetypeElement<TValue>[] Elements => ref _elements.GetValueArray();

		public int Count => _elements.Count;
		public int IndexesCapacity => _elementIndexes.Capacity;

		public Archetype(OrderedSparseSet<ArchetypeElement<TValue>>.ResetAction resetAction = null,
			EntityDestroyAction entityDestroyEvent = null) : this(ServiceLocator<EntitiesState>.Instance.EntitiesCapacity, resetAction, entityDestroyEvent)
		{

		}


		public Archetype(int elementsCount, OrderedSparseSet<ArchetypeElement<TValue>>.ResetAction resetAction = null,
			EntityDestroyAction entityDestroyEvent = null) : this(elementsCount, ServiceLocator<EntitiesState>.Instance.EntitiesCapacity, resetAction, entityDestroyEvent)
		{

		}

		private Archetype(int elementsCount, int entitiesCapacity, OrderedSparseSet<ArchetypeElement<TValue>>.ResetAction resetAction = null, EntityDestroyAction entityDestroyEvent = null)
		{
			_elements = new OrderedSparseSet<ArchetypeElement<TValue>>(elementsCount, resetAction);
			_elementIndexes = new SimpleList<int>(entitiesCapacity, -1);

			OnEntityDestroyEvent = entityDestroyEvent;
			if (OnEntityDestroyEvent == null)
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent += RemoveElement;
			else
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent += OnEntityDestroy;
		}

		private ref int GetIndexId(Entity entity)
		{
			_elementIndexes.Expand(entity.id + 1, -1);
			return ref _elementIndexes[entity.id];
		}

		public ref readonly TValue ReadElement(Entity entity)
		{
			var indexId = GetIndexId(entity);
			if (indexId < 0)
				return ref DEFAULT;
			return ref _elements.GetValue(indexId).value;
		}

		public bool HasElement(Entity entity)
		{
			return GetIndexId(entity) >= 0;
		}

		public void RemoveElement(Entity entity)
		{
			ref var indexId = ref GetIndexId(entity);
			if (indexId < 0)
				return;
			_elements.ReleaseIndexId(indexId);
			indexId = -1;
		}

		private void OnEntityDestroy(Entity entity)
		{
			ref var indexId = ref GetIndexId(entity);
			if (indexId < 0)
				return;

			OnEntityDestroyEvent?.Invoke(ref _elements.GetValue(indexId));

			if (indexId < 0)
				return;
			_elements.ReleaseIndexId(indexId);
			indexId = -1;
		}

		public ref TValue GetElement(Entity entity)
		{
			ref var indexId = ref GetIndexId(entity);
			if (indexId < 0)
			{
				indexId = _elements.AllocateIndexId();
				ref var element = ref _elements.GetValue(indexId);

				element = new ArchetypeElement<TValue>(entity, default);
				return ref element.value;
			}
			else
			{
				ref var element = ref _elements.GetValue(indexId);
				if (element.entity != entity)
				{
					element = new ArchetypeElement<TValue>(entity, default);
				}
				return ref element.value;
			}
		}

		public void Clear()
		{
			var valueArray = _elements.GetValueArray();
			var count = _elements.Count;

			for (var i = 0; i < count; i++)
			{
				_elementIndexes[valueArray[i].entity.id] = -1;
			}

			_elements.ClearFast();
		}

		~Archetype()
		{
			if (ServiceLocator<EntitiesState>.Instance == null)
				return;

			if (OnEntityDestroyEvent == null)
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent -= RemoveElement;
			else
				ServiceLocator<EntitiesState>.Instance.EntityDestroyEvent -= OnEntityDestroy;
		}
	}
}