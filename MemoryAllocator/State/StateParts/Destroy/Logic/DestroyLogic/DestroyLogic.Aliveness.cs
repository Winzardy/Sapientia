using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyLogic
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref AliveDuration GetAliveDuration(Entity entity)
		{
			return ref _aliveDurationSet.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref AliveDuration TryGetAliveDuration(Entity entity, out bool isExist)
		{
			return ref _aliveDurationSet.TryGetElement(entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetTimeDebt(Entity entity)
		{
			return _aliveTimeDebtSet.ReadElement(entity).timeDebt.GetValue(_worldState.Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetTimeDebt(Entity entity, out float timeDebt)
		{
			return _aliveTimeDebtSet.ReadElement(entity).timeDebt.TryGetValue(_worldState.Tick, out timeDebt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetTimeDebt(Entity entity, float timeDebt)
		{
			_aliveTimeDebtSet.GetElement(entity).timeDebt = new OneShotValue<float>(_worldState.Tick, timeDebt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive(Entity entity)
		{
			if (_destroyRequestSet.HasElement(entity))
				return false;
			if (_killRequestSet.HasElement(entity))
				return false;
			return _entityStatePart.Value().IsEntityExist(_worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsExist(Entity entity)
		{
			return _entityStatePart.Value().IsEntityExist(_worldState, entity);
		}
	}
}
