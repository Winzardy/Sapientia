using Sapientia.Data;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyLogic : IInitializableService
	{
		private WorldState _worldState;

		private SafePtr<EntityStatePart> _entityStatePart;

		private ComponentSetContext<DestroyComponent> _destroyElementSet;
		private ComponentSetContext<DestroyRequest> _destroyRequestSet;

		private ComponentSetContext<KillRequest> _killRequestSet;
		private ComponentSetContext<KillCallbackComponent> _killCallbackSet;
		private ComponentSetContext<DelayKillRequest> _delayKillRequestSet;

		private ComponentSetContext<AliveDuration> _aliveDurationSet;
		private ComponentSetContext<AliveTimeDebt> _aliveTimeDebtSet;

		private ComponentSetContext<DisabledComponent> _disabledSet;
		private ComponentSetContext<ActivityCallbackComponent> _activityCallbackSet;

		void IInitializableService.Initialize(WorldState worldState)
		{
			_worldState = worldState;

			_entityStatePart = worldState.GetServicePtr<EntityStatePart>();

			_destroyElementSet = new ComponentSetContext<DestroyComponent>(worldState);
			_destroyRequestSet = new ComponentSetContext<DestroyRequest>(worldState);

			_killCallbackSet = new ComponentSetContext<KillCallbackComponent>(worldState);
			_killRequestSet = new ComponentSetContext<KillRequest>(worldState);
			_delayKillRequestSet = new ComponentSetContext<DelayKillRequest>(worldState);

			_aliveDurationSet = new ComponentSetContext<AliveDuration>(worldState);
			_aliveTimeDebtSet = new ComponentSetContext<AliveTimeDebt>(worldState);

			_disabledSet = new ComponentSetContext<DisabledComponent>(worldState);
			_activityCallbackSet = new ComponentSetContext<ActivityCallbackComponent>(worldState);
		}
	}
}
