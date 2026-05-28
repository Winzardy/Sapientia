#nullable disable
using System;

namespace Sapientia.Conditions
{
	public class ConditionalObjectSurrogate<TContext> : IStatefulConditionalObject<TContext>, IDisposable
	{
#if UNITY_EDITOR
		private Guid _guid;
#endif
		private TContext _context;
		private Action<bool> _stateSwitchAction;

		public string Id { get; private set; }
		public bool IsActive { get; private set; }
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// If both activation and deactivation conditions are currently fulfilled,
		/// and object is in an inactive state - activation will be ignored.
		/// </summary>
		public bool DeactivationPriority { get; set; }

		public ICondition<TContext> ActivationCondition { get; }
		public ICondition<TContext> DeactivationCondition { get; }

		public bool CanBeActivated { get => CanSetActive(true); }
		public bool CanBeDeactivated { get => CanSetActive(false); }

		public event Action<IStatefulConditionalObject<TContext>> StateUpdated;
		public event Action<ConditionalObjectSurrogate<TContext>> Disposed;

		public ConditionalObjectSurrogate(
			ICondition<TContext> activationCondition,
			ICondition<TContext> deactivationCondition,
			Action<bool> stateSwitchAction = null)
		{
#if UNITY_EDITOR
			_guid = Guid.NewGuid();
#endif
			_stateSwitchAction = stateSwitchAction;

			ActivationCondition = activationCondition;
			DeactivationCondition = deactivationCondition;
		}

		public ConditionalObjectSurrogate(
			TContext context,
			ICondition<TContext> activationCondition,
			ICondition<TContext> deactivationCondition,
			Action<bool> stateSwitchAction = null) :
			this(activationCondition, deactivationCondition, stateSwitchAction)
		{
			_context = context;
			SetActive(this.CanBeActivated(_context));
		}

		public ConditionalObjectSurrogate(
			TContext context,
			ICondition<TContext> activationCondition,
			ICondition<TContext> deactivationCondition,
			bool isActive,
			Action<bool> stateSwitchAction = null) :
			this(activationCondition, deactivationCondition, stateSwitchAction)
		{
			_context = context;
			IsActive = isActive;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;

			IsDisposed = true;
			Disposed?.Invoke(this);
		}

		public void SetId(string id)
		{
			this.Id = id;
		}

		public void SetActive(bool isActive, bool silently = false)
		{
			this.IsActive = isActive;

			if (!silently)
			{
				_stateSwitchAction?.Invoke(isActive);
				StateUpdated?.Invoke(this);
			}
		}

		public bool CanSetActive(bool active) => CanSetActive(active, out _, false);
		public bool CanSetActive(bool active, out Exception exception, bool generateException = true)
		{
			if (IsActive == active)
			{
				exception = generateException ? new Exception(
				   $"Could not change state for surrogate [ {Id} ] to [ {active} ] - " +
				   $"state is identical.") : null;

				return false;
			}

			if (_context != null)
			{
				if (active && (DeactivationPriority ? !this.CanBeActivated(_context) : !ActivationCondition.IsFulfilled<TContext>(_context)))
				{
					exception = generateException ? new Exception(
					   $"Could not change state for surrogate [ {Id} ] to [ {active} ] - " +
					   $"activation conditions are not met.") : null;

					return false;
				}
				
				if (!active && !this.CanBeDeactivated(_context))
				{
					exception = generateException ? new Exception(
					   $"Could not change state for surrogate [ {Id} ] to [ {active} ] - " +
					   $"deactivation conditions are not met.") : null;

					return false;
				}
			}

			exception = null;
			return true;
		}

		public override string ToString()
		{
			var id = this.Id != null ?
				$"ID [ {this.Id} ] " :
#if UNITY_EDITOR
				$"GUID [ {_guid} ] ";
#else
                null;
#endif

			return
				id +
				$"Is Active [ {IsActive} ] " +
				$"\nActivation Conditions: {ActivationCondition} " +
				$"\nDeactivation Conditions: {DeactivationCondition}";
		}
	}
}
