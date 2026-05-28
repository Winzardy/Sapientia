#nullable disable
using System;
using JetBrains.Annotations;
using Sapientia.Collections;
using Sapientia.Evaluators;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract partial class Condition<TContext> : Evaluator<TContext, bool>, ICondition<TContext>
	{
		bool IEvaluator<TContext, bool>.Evaluate(TContext context)
			=> EvaluateInternal(context);

		bool ICondition<TContext>.IsFulfilled(TContext context)
			=> EvaluateInternal(context);

		protected virtual bool EvaluateInternal(TContext context) => OnEvaluate(context);

		public static implicit operator Condition<TContext>(bool value) => value
			? null
			: new RejectCondition<TContext>();
	}

	/// <summary>
	/// Используется в основном для инспектора, так как <c>condition == null</c> и так <c>true</c>
	/// </summary>
	public class NoneCondition<TContext> : Condition<TContext>
	{
		protected override bool OnEvaluate(TContext _) => true;
	}

	public interface IRejectCondition
	{
	}

	[Serializable]
	public class RejectCondition<TContext> : Condition<TContext>, IRejectCondition
	{
		protected override bool OnEvaluate(TContext _) => false;
	}

	public static class ConditionUtility
	{
		public static bool IsReject<TContext>(this ICondition<TContext> condition)
			=> condition is IRejectCondition;

		public static bool IsFulfilled<TContext>([CanBeNull] this ICondition<TContext> condition, TContext context)
			=> condition == null || condition.Evaluate(context);

		/// <summary>
		/// Object was presumably activated by conditions, or in a state where it is supposed to be active.
		/// False is activation conditions are empty.
		/// </summary>
		public static bool WasActivated<TContext>(bool isActive, ICondition<TContext> activationCondition, TContext context)
		{
			return
				isActive &&
				!activationCondition.IsNullOrEmpty() &&
				IsFulfilled(activationCondition, context);
		}

		/// <summary>
		/// Object was presumably deactivated by conditions, or in a state where it is supposed to be inactive.
		/// False is deactivation conditions are empty.
		/// </summary>
		public static bool WasDeactivated<TContext>(bool isActive, ICondition<TContext> deactivationCondition, TContext context)
		{
			return
				!isActive &&
				!deactivationCondition.IsNullOrEmpty() &&
				IsFulfilled(deactivationCondition, context);
		}

		/// <summary>
		/// Checks if deactivation condition is fullfilled and skips activation check if it is.
		/// Use it in case you want to prevent an object to activate and then deactivate immediately.
		/// </summary>
		public static bool CanBeActivated<TContext>(bool isActive, ICondition<TContext> activationCondition, ICondition<TContext> deactivationCondition, TContext context)
		{
			if (isActive || WasDeactivated(isActive, deactivationCondition, context))
				return false;

			return IsFulfilled(activationCondition, context);
		}

		/// <summary>
		/// Checks if object is active, and returns false if it is already in an inactive state.
		/// Also returns false, if deactivation conditions are empty.
		/// </summary>
		public static bool CanBeDeactivated<TContext>(bool isActive, ICondition<TContext> deactivationCondition, TContext context)
		{
			if (!isActive)
				return false;

			return
				!deactivationCondition.IsNullOrEmpty() &&
				IsFulfilled(deactivationCondition, context);
		}

		public static bool WasActivated<TContext>(this IStatefulConditionalObject<TContext> obj, TContext context)
			=> WasActivated(obj.IsActive, obj.ActivationCondition, context);

		public static bool WasDeactivated<TContext>(this IStatefulConditionalObject<TContext> obj, TContext context)
			=> WasDeactivated(obj.IsActive, obj.DeactivationCondition, context);

		public static bool CanBeActivated<TContext>(this IStatefulConditionalObject<TContext> obj, TContext context)
			=> CanBeActivated(obj.IsActive, obj.ActivationCondition, obj.DeactivationCondition, context);

		public static bool CanBeDeactivated<TContext>(this IStatefulConditionalObject<TContext> obj, TContext context)
			=> CanBeDeactivated(obj.IsActive, obj.DeactivationCondition, context);
	}
}
