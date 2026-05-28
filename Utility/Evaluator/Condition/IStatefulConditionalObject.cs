#nullable disable
using System;

namespace Sapientia.Conditions
{
	public interface IStatefulConditionalObject<TContext>
	{
		public bool IsActive { get; }
		public ICondition<TContext> ActivationCondition { get; }
		public ICondition<TContext> DeactivationCondition { get; }

		public event Action<IStatefulConditionalObject<TContext>> StateUpdated;
	}
}
