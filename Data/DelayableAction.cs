using System;
using Sapientia.Collections;

namespace Sapientia.Data
{
	public class DelayableAction : AsyncClass
	{
		private event Action ActionEvent = null;

		private int _invocationCount = 0;

		public void ImmediatelyInvoke()
		{
			ActionEvent?.Invoke();
		}

		public void DelayInvoke()
		{
			using var scope = GetBusyScope();
			_invocationCount++;
		}

		public void InvokeDelayed()
		{
			using var scope = GetBusyScope();

			if (ActionEvent == null)
			{
				_invocationCount = 0;
				return;
			}

			while (_invocationCount > 0)
			{
				ActionEvent.Invoke();
				_invocationCount--;
			}
		}

		public void Subscribe(Action action)
		{
			ActionEvent += action;
		}

		public void UnSubscribe(Action action)
		{
			ActionEvent -= action;
		}

		public static DelayableAction operator +(DelayableAction delayableAction, Action action)
		{
			using var scope = delayableAction.GetBusyScope();

			delayableAction.ActionEvent += action;
			return delayableAction;
		}

		public static DelayableAction operator -(DelayableAction delayableAction, Action action)
		{
			using var scope = delayableAction.GetBusyScope();

			delayableAction.ActionEvent -= action;
			return delayableAction;
		}
	}

	public class DelayableAction<TContext> : AsyncClass
	{
		private event Action<TContext> ActionEvent = null;

		private readonly SimpleList<TContext> _invocationContextList = new ();

		public void ImmediatelyInvoke(TContext context)
		{
			ActionEvent?.Invoke(context);
		}

		public void DelayInvoke(TContext context)
		{
			using var scope = GetBusyScope();
			_invocationContextList.Add(context);
		}

		public void InvokeDelayed()
		{
			using var scope = GetBusyScope();

			if (ActionEvent == null)
			{
				_invocationContextList.Clear();
				return;
			}

			for (var i = 0; i < _invocationContextList.Count; i++)
			{
				var context = _invocationContextList[i];
				ActionEvent.Invoke(context);
			}
			_invocationContextList.Clear();
		}

		public void Subscribe(Action<TContext> action)
		{
			ActionEvent += action;
		}

		public void UnSubscribe(Action<TContext> action)
		{
			ActionEvent -= action;
		}

		public static DelayableAction<TContext> operator +(DelayableAction<TContext> delayableAction, Action<TContext> action)
		{
			using var scope = delayableAction.GetBusyScope();

			delayableAction.ActionEvent += action;
			return delayableAction;
		}

		public static DelayableAction<TContext> operator -(DelayableAction<TContext> delayableAction, Action<TContext> action)
		{
			using var scope = delayableAction.GetBusyScope();

			delayableAction.ActionEvent -= action;
			return delayableAction;
		}
	}
}