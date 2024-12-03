using System;

namespace Sapientia.Messaging
{
	/// <summary>
	/// Объединяет событие и передачу сообщение (<see cref="Messenger"/>) в один объект.
	/// Так же как и у события есть подписка и отписка, а так же Invoke
	/// </summary>
	public struct Mailer<TMessage> where TMessage : struct
	{
		private MessengerHub _hub;

		private event Action _action;
		private event Receiver<TMessage> _receiver;

		private TMessage _last;

		public Mailer(MessengerHub hub = null) : this() => Setup(hub);

		public void Setup(MessengerHub hub) => _hub = hub;

		public void Subscribe(Receiver<TMessage> receiver) => _receiver += receiver;
		public void Unsubscribe(Receiver<TMessage> receiver) => _receiver -= receiver;

		public void Subscribe(Action action) => _action += action;
		public void Unsubscribe(Action action) => _action -= action;

		public void Invoke() => InvokeInternal(ref _last);

		public void Invoke(ref TMessage msg, bool useLast = false)
		{
			if (useLast)
			{
				_last = msg;
				InvokeInternal(ref _last);
				return;
			}

			InvokeInternal(ref msg);
		}

		private void InvokeInternal(ref TMessage msg)
		{
			_action?.Invoke();
			_receiver?.Invoke(in msg);

			if (_hub != null)
			{
				_hub.Send(ref msg);
				return;
			}

			Messenger.Send(ref msg);
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage> operator +(Mailer<TMessage> mailer, Receiver<TMessage> receiver)
		{
			mailer.Subscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage> operator -(Mailer<TMessage> mailer, Receiver<TMessage> receiver)
		{
			mailer.Unsubscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage> operator +(Mailer<TMessage> mailer, Action action)
		{
			mailer.Subscribe(action);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage> operator -(Mailer<TMessage> mailer, Action action)
		{
			mailer.Unsubscribe(action);
			return mailer;
		}
	}

	/// <summary>
	/// Объединяет событие и передачу сообщение (<see cref="Messenger"/>) в один объект.
	/// Так же как и у события есть подписка и отписка, а так же Invoke
	/// </summary>
	public struct Mailer<TMessage, T1> where TMessage : struct
	{
		public delegate void CustomAction(in T1 value);

		public delegate T1 ToValueDelegate(in TMessage input);

		public delegate TMessage ToMessageDelegate(in T1 input);

		private MessengerHub _hub;

		private event CustomAction _action;
		private event Receiver<TMessage> _receiver;

		private ToMessageDelegate _toMsg;
		private ToValueDelegate _toValue;

		private TMessage _last;

		public Mailer(ToMessageDelegate toMsg,
			ToValueDelegate toValue,
			MessengerHub hub = null)
		{
			_toMsg = toMsg;
			_toValue = toValue;

			_hub = hub;
			_last = default;
			_action = null;
			_receiver = null;
		}

		public void Setup(MessengerHub hub) => _hub = hub;

		public void Subscribe(Receiver<TMessage> receiver) => _receiver += receiver;
		public void Unsubscribe(Receiver<TMessage> receiver) => _receiver -= receiver;

		public void Subscribe(CustomAction action) => _action += action;
		public void Unsubscribe(CustomAction action) => _action -= action;

		public void Invoke(in T1 value)
		{
			_last = _toMsg.Invoke(in value);
			InvokeInternal(in value, ref _last);
		}

		public void Invoke(ref TMessage msg, bool useLast = false)
		{
			if (useLast)
			{
				_last = msg;

				InvokeInternal(_toValue.Invoke(in _last), ref _last);
				return;
			}

			InvokeInternal(_toValue.Invoke(in msg), ref msg);
		}

		private void InvokeInternal(in T1 value, ref TMessage msg)
		{
			_action?.Invoke(in value);
			_receiver?.Invoke(in msg);

			if (_hub != null)
			{
				_hub.Send(ref msg);
				return;
			}

			Messenger.Send(ref msg);
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage, T1> operator +(Mailer<TMessage, T1> mailer, Receiver<TMessage> receiver)
		{
			mailer.Subscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage, T1> operator -(Mailer<TMessage, T1> mailer, Receiver<TMessage> receiver)
		{
			mailer.Unsubscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage, T1> operator +(Mailer<TMessage, T1> mailer, CustomAction action)
		{
			mailer.Subscribe(action);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage, T1> operator -(Mailer<TMessage, T1> mailer, CustomAction action)
		{
			mailer.Unsubscribe(action);
			return mailer;
		}
	}

	/// <summary>
	/// Объединяет событие и передачу сообщение (<see cref="Messenger"/>) в один объект.
	/// Так же как и у события есть подписка и отписка, а так же Invoke
	/// </summary>
	public struct Mailer<TMessage, T1, T2> where TMessage : struct
	{
		public delegate void CustomAction(in T1 first, in T2 second);

		public delegate (T1 first, T2 second) ToValueDelegate(in TMessage input);

		public delegate TMessage ToMessageDelegate(in T1 first, in T2 second);

		private MessengerHub _hub;

		private event CustomAction _action;
		private event Receiver<TMessage> _receiver;

		private ToMessageDelegate _toMsg;
		private ToValueDelegate _toValue;

		private TMessage _last;

		public Mailer(ToMessageDelegate toMsg,
			ToValueDelegate toValue,
			MessengerHub hub = null)
		{
			_toMsg = toMsg;
			_toValue = toValue;

			_hub = hub;
			_last = default;
			_action = null;
			_receiver = null;
		}

		public void Setup(MessengerHub hub) => _hub = hub;

		public void Subscribe(Receiver<TMessage> receiver) => _receiver += receiver;
		public void Unsubscribe(Receiver<TMessage> receiver) => _receiver -= receiver;

		public void Subscribe(CustomAction action) => _action += action;
		public void Unsubscribe(CustomAction action) => _action -= action;

		public void Invoke(in T1 value, in T2 value2)
		{
			_last = _toMsg.Invoke(in value, in value2);
			InvokeInternal(in value, in value2, ref _last);
		}

		public void Invoke(ref TMessage msg, bool useLast = false)
		{
			if (useLast)
			{
				_last = msg;

				var (lastFirst, lastSecond) = _toValue.Invoke(in _last);
				InvokeInternal(in lastFirst, in lastSecond, ref _last);
				return;
			}

			var (first, second) = _toValue.Invoke(in msg);
			InvokeInternal(in first, in second, ref msg);
		}

		private void InvokeInternal(in T1 first, in T2 second, ref TMessage msg)
		{
			_action?.Invoke(in first, in second);
			_receiver?.Invoke(in msg);

			if (_hub != null)
			{
				_hub.Send(ref msg);
				return;
			}

			Messenger.Send(ref msg);
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage, T1, T2> operator +(Mailer<TMessage, T1, T2> mailer, Receiver<TMessage> receiver)
		{
			mailer.Subscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage, T1, T2> operator -(Mailer<TMessage, T1, T2> mailer, Receiver<TMessage> receiver)
		{
			mailer.Unsubscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage, T1, T2> operator +(Mailer<TMessage, T1, T2> mailer, CustomAction action)
		{
			mailer.Subscribe(action);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage, T1, T2> operator -(Mailer<TMessage, T1, T2> mailer, CustomAction action)
		{
			mailer.Unsubscribe(action);
			return mailer;
		}
	}

	/// <summary>
	/// Объединяет событие и передачу сообщение (<see cref="Messenger"/>) в один объект.
	/// Так же как и у события есть подписка и отписка, а так же Invoke
	/// </summary>
	public struct Mailer<TMessage, T1, T2, T3> where TMessage : struct
	{
		public delegate void CustomAction(in T1 first, in T2 second, in T3 third);

		public delegate (T1 first, T2 second, T3 third) ToValueDelegate(in TMessage input);

		public delegate TMessage ToMessageDelegate(in T1 first, in T2 second, in T3 third);

		private MessengerHub _hub;

		private event CustomAction _action;
		private event Receiver<TMessage> _receiver;

		private ToMessageDelegate _toMsg;
		private ToValueDelegate _toValue;

		private TMessage _last;

		public Mailer(ToMessageDelegate toMsg,
			ToValueDelegate toValue,
			MessengerHub hub = null)
		{
			_toMsg = toMsg;
			_toValue = toValue;

			_hub = hub;
			_last = default;
			_action = null;
			_receiver = null;
		}

		public void Setup(MessengerHub hub) => _hub = hub;

		public void Subscribe(Receiver<TMessage> receiver) => _receiver += receiver;
		public void Unsubscribe(Receiver<TMessage> receiver) => _receiver -= receiver;

		public void Subscribe(CustomAction action) => _action += action;
		public void Unsubscribe(CustomAction action) => _action -= action;

		public void Invoke(in T1 first, in T2 second, in T3 third)
		{
			_last = _toMsg.Invoke(in first, in second, in third);
			InvokeInternal(in first, in second, in third, ref _last);
		}

		public void Invoke(ref TMessage msg, bool useLast = false)
		{
			if (useLast)
			{
				_last = msg;

				var (lastFirst, lastSecond, lastThird) = _toValue.Invoke(in _last);
				InvokeInternal(in lastFirst, in lastSecond, in lastThird, ref _last);
				return;
			}

			var (first, second, third) = _toValue.Invoke(in msg);
			InvokeInternal(in first, in second, in third, ref msg);
		}

		private void InvokeInternal(in T1 first, in T2 second, in T3 third, ref TMessage msg)
		{
			_action?.Invoke(in first, in second, third);
			_receiver?.Invoke(in msg);

			if (_hub != null)
			{
				_hub.Send(ref msg);
				return;
			}

			Messenger.Send(ref msg);
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage, T1, T2, T3> operator +(Mailer<TMessage, T1, T2, T3> mailer, Receiver<TMessage> receiver)
		{
			mailer.Subscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage, T1, T2, T3> operator -(Mailer<TMessage, T1, T2, T3> mailer, Receiver<TMessage> receiver)
		{
			mailer.Unsubscribe(receiver);
			return mailer;
		}

		/// <summary>
		/// Аналог подписки
		/// </summary>
		public static Mailer<TMessage, T1, T2, T3> operator +(Mailer<TMessage, T1, T2, T3> mailer, CustomAction action)
		{
			mailer.Subscribe(action);
			return mailer;
		}

		/// <summary>
		/// Аналог отписки
		/// </summary>
		public static Mailer<TMessage, T1, T2, T3> operator -(Mailer<TMessage, T1, T2, T3> mailer, CustomAction action)
		{
			mailer.Unsubscribe(action);
			return mailer;
		}
	}
}
