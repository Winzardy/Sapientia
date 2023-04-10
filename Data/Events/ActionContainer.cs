using System;

namespace Sapientia.Data.Events
{
	public class ActionContainer<TContext>
	{
		private event Action<TContext> ActionEvent;

		public void Invoke(TContext context)
		{
			ActionEvent?.Invoke(context);
		}

		public void Subscribe(Action<TContext> action)
		{
			ActionEvent += action;
		}

		public void UnSubscribe(Action<TContext> action)
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

		public static ActionContainer<TContext> operator +(ActionContainer<TContext> container, Action<TContext> action)
		{
			container ??= new();
			container.ActionEvent += action;
			return container;
		}

		public static ActionContainer<TContext> operator -(ActionContainer<TContext> container, Action<TContext> action)
		{
			if (container != null)
				container.ActionEvent -= action;
			return container;
		}
	}
}