namespace Sapientia.MemoryAllocator.State
{
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
}
