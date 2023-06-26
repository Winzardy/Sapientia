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
			using var scope = GetBusyScope();
			ActionEvent?.Invoke();
		}

		public void DelayInvoke()
		{
			_invocationCount++;
		}

		public void DelayInvokeInterlocked()
		{
			using var scope = GetBusyScope();
			_invocationCount++;
		}

		public bool InvokeDelayedInterlocked()
		{
			using var scope = GetBusyScope();
			return InvokeDelayed();
		}

		public bool InvokeDelayed()
		{
			if (ActionEvent == null || _invocationCount < 1)
			{
				_invocationCount = 0;
				return false;
			}

			do
			{
				ActionEvent.Invoke();
				_invocationCount--;
			} while (_invocationCount > 0);

			return true;
		}

		public bool InvokeDelayedOnceInterlocked()
		{
			using var scope = GetBusyScope();
			return InvokeDelayedOnce();
		}

		public bool InvokeDelayedOnce()
		{
			if (ActionEvent == null || _invocationCount < 1)
			{
				_invocationCount = 0;
				return false;
			}

			ActionEvent.Invoke();
			_invocationCount = 0;

			return true;
		}

		public void ResetInterlocked()
		{
			using var scope = GetBusyScope();
			ActionEvent = null;
		}

		public void SubscribeInterlocked(Action action)
		{
			using var scope = GetBusyScope();
			ActionEvent += action;
		}

		public void UnSubscribeInterlocked(Action action)
		{
			using var scope = GetBusyScope();
			ActionEvent -= action;
		}

		public void Subscribe(Action action)
		{
			ActionEvent += action;
		}

		public void UnSubscribe(Action action)
		{
			ActionEvent -= action;
		}

		public void SubscribeInterlocked(DelayableAction action)
		{
			using var scopeA = GetBusyScope();
			using var scopeB = action.GetBusyScope();
			ActionEvent += action.ActionEvent;
		}

		public void UnSubscribeInterlocked(DelayableAction action)
		{
			using var scopeA = GetBusyScope();
			using var scopeB = action.GetBusyScope();
			ActionEvent -= action.ActionEvent;
		}

		public void Subscribe(DelayableAction action)
		{
			ActionEvent += action.ActionEvent;
		}

		public void UnSubscribe(DelayableAction action)
		{
			ActionEvent -= action.ActionEvent;
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

		public void DelayInvokeInterlocked(TContext context)
		{
			using var scope = GetBusyScope();
			_invocationContextList.AddWithoutExpand(context);
		}

		public void InvokeDelayedInterlocked()
		{
			using var scope = GetBusyScope();

			if (ActionEvent == null)
			{
				_invocationContextList.ClearPartial();
				return;
			}

			for (var i = 0; i < _invocationContextList.Count; i++)
			{
				var context = _invocationContextList[i];
				ActionEvent.Invoke(context);
			}
			_invocationContextList.ClearPartial();
		}

		public void SubscribeInterlocked(Action<TContext> action)
		{
			using var scope = GetBusyScope();
			ActionEvent += action;
		}

		public void UnSubscribeInterlocked(Action<TContext> action)
		{
			using var scope = GetBusyScope();
			ActionEvent -= action;
		}

		public void Subscribe(Action<TContext> action)
		{
			ActionEvent += action;
		}

		public void UnSubscribe(Action<TContext> action)
		{
			ActionEvent -= action;
		}

		public void SubscribeInterlocked(DelayableAction<TContext> action)
		{
			using var scopeA = GetBusyScope();
			using var scopeB = action.GetBusyScope();
			ActionEvent += action.ActionEvent;
		}

		public void UnSubscribeInterlocked(DelayableAction<TContext> action)
		{
			using var scopeA = GetBusyScope();
			using var scopeB = action.GetBusyScope();
			ActionEvent -= action.ActionEvent;
		}

		public void Subscribe(DelayableAction<TContext> action)
		{
			ActionEvent += action.ActionEvent;
		}

		public void UnSubscribe(DelayableAction<TContext> action)
		{
			ActionEvent -= action.ActionEvent;
		}
	}
}