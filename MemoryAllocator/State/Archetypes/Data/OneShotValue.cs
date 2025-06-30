using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator.State
{
	public readonly struct OneShotValue<TValue>
	{
		private readonly uint _version;
		private readonly TValue _value;

		public OneShotValue(uint worldTick, TValue value)
		{
			_version = worldTick;
			_value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValid(uint worldTick)
		{
			return _version == worldTick;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(uint worldTick, out TValue value)
		{
			if (IsValid(worldTick))
			{
				value = _value;
				return true;
			}
			value = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TValue GetValue(uint worldTick)
		{
			if (IsValid(worldTick))
				return _value;
			return default;
		}
	}
}
