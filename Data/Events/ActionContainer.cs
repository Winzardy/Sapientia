using System;

namespace Sapientia.Data.Events
{
	public struct ActionContainer<TContext>
	{
		private event Action<TContext> ActionEvent;

		public void Invoke(TContext context)
		{
			ActionEvent?.Invoke(context);
		}

		public readonly void Subscribe(Action<TContext> action)
		{
			ActionEvent += action;
		}

		public readonly void UnSubscribe(Action<TContext> action)
		{
			ActionEvent -= action;
		}

		public static implicit operator ActionContainer<TContext>(Action<TContext> action)
		{
			return new ActionContainer<TContext>
			{
				ActionEvent = action,
			};
		}

		public static implicit operator Action<TContext>(ActionContainer<TContext> container)
		{
			return container.Invoke;
		}

		public static ActionContainer<TContext> operator +(in ActionContainer<TContext> container, Action<TContext> action)
		{
			container.ActionEvent += action;
			return container;
		}

		public static ActionContainer<TContext> operator -(in ActionContainer<TContext> container, Action<TContext> action)
		{
			container.ActionEvent -= action;
			return container;
		}
	}
}