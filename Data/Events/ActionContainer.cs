using System;

namespace Sapientia.Data.Events
{
	public class ActionContainer<TContext>
	{
		public bool ExecuteIfInvoked { get; private set; }
		public bool IsInvoked { get; private set; }
		public TContext PreviousContext { get; private set; }

		private event Action<TContext> ActionEvent;

		public ActionContainer(bool executeIfInvoked = false)
		{
			ExecuteIfInvoked = executeIfInvoked;
		}

		public void Invoke(TContext context)
		{
			ActionEvent?.Invoke(context);
			IsInvoked = true;
			PreviousContext = context;
		}

		public void Subscribe(Action<TContext> action)
		{
			ActionEvent += action;
			if (ExecuteIfInvoked && IsInvoked)
				action?.Invoke(PreviousContext);
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
			if (container.ExecuteIfInvoked && container.IsInvoked)
				action?.Invoke(container.PreviousContext);
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