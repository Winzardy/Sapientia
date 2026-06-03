namespace Sapientia.MemoryAllocator.State
{
	public struct OptionalValue<TValue>
	{
		public static OptionalValue<TValue> Empty => default;

		public TValue value;
		private byte _isEnabled;

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

		public readonly TValue GetValue()
		{
			if (_isEnabled == 1)
				return value;
			return default;
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

		public override int GetHashCode()
		{
			var hashCode = value.GetHashCode();
			if (_isEnabled == 0)
				hashCode = ~hashCode;
			return hashCode;
		}
	}
}
