using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public struct EmptyEventContext {}

	public class MultiEvent<TIntFlag, TContext> : AsyncClass where TIntFlag: unmanaged, Enum
	{
		public delegate ChangeContextScope GetChangeContextScopeDelegate(TIntFlag eventType);
		public delegate void OnEventDelegate(TIntFlag eventType);

		public event Action<TContext> AllEventsTriggered;

		private TContext _context;
		private int _state;

		public MultiEvent(TContext context = default)
		{
			_context = context;
			_state = ~EnumExtensions<TIntFlag>.filledFlag;
		}

		public void OnEvent(TIntFlag eventType)
		{
			using (GetBusyScope())
			{
				InnerOnEvent(eventType);
			}
		}

		private void InnerOnEvent(TIntFlag eventType)
		{
			_state |= eventType.ToInt();
			if (_state == ~0)
				InnerInvoke();
		}

		public ChangeContextScope GetChangeContextScope(TIntFlag eventType)
		{
			return new ChangeContextScope(this, eventType);
		}

		public void Invoke()
		{
			using (GetBusyScope())
			{
				InnerInvoke();
			}
		}

		private void InnerInvoke()
		{
			AllEventsTriggered?.Invoke(_context);
		}

		public void Reset()
		{
			using (GetBusyScope())
			{
				_state = ~EnumExtensions<TIntFlag>.filledFlag;
			}
		}

		public ref struct ChangeContextScope
		{
			public TContext context;

			private readonly MultiEvent<TIntFlag, TContext> _multiEvent;
			private readonly TIntFlag _eventType;

			public ChangeContextScope(MultiEvent<TIntFlag, TContext> multiEvent, TIntFlag eventType)
			{
				_multiEvent = multiEvent;
				_eventType = eventType;

				_multiEvent.SetBusy();
				context = _multiEvent._context;
			}

			public void SetContext(TContext newContext)
			{
				context = newContext;
			}

			public void Dispose()
			{
				_multiEvent._context = context;
				_multiEvent.InnerOnEvent(_eventType);

				_multiEvent.SetFree();
			}
		}
	}
}