namespace Sapientia.MemoryAllocator.State
{
	public readonly unsafe struct OneShotValue<TValue>
	{
		private readonly uint _version;
		private readonly TValue _value;

		private OneShotValue(TValue value)
		{
			_version = AllocatorManager.CurrentAllocatorPtr->serviceRegistry.GetService<World>().Tick;
			_value = value;
		}

		public readonly bool IsValid()
		{
			return _version == AllocatorManager.CurrentAllocatorPtr->serviceRegistry.GetService<World>().Tick;
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
}
