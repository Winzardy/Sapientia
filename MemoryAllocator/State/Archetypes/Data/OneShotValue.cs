namespace Sapientia.MemoryAllocator.State
{
	public readonly struct OneShotValue<TValue>
	{
		private readonly uint _version;
		private readonly TValue _value;

		private OneShotValue(TValue value)
		{
			_version = WorldManager.CurrentWorld.GetService<WorldState>().Tick;
			_value = value;
		}

		public readonly bool IsValid()
		{
			return _version == WorldManager.CurrentWorld.GetService<WorldState>().Tick;
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
