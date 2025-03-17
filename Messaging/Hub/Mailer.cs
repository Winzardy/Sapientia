namespace Sapientia.Messaging
{
	/// <summary>
	/// Объединяет событие и передачу сообщение (<see cref="Messenger"/>) в один объект.
	/// Так же как и у события есть подписка и отписка, а так же Invoke
	/// </summary>
	public struct Mailer<TMessage> : IReactiveProperty<TMessage>
		where TMessage : struct
	{
		private MessengerHub _hub;

		private event Receiver<TMessage> _receiver;

		private TMessage _last;
		public TMessage Value => _last;

		public Mailer(MessengerHub hub = null) : this() => Setup(hub);

		public void Setup(MessengerHub hub) => _hub = hub;

		public void Subscribe(Receiver<TMessage> receiver, bool invokeOnSubscribe = true)
		{
			if (invokeOnSubscribe)
				receiver?.Invoke(_last);
			_receiver += receiver;
		}

		public void Unsubscribe(Receiver<TMessage> receiver) => _receiver -= receiver;

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
			_receiver?.Invoke(in msg);

			if (_hub != null)
			{
				_hub.Send(ref msg);
				return;
			}

			Messenger.Send(ref msg);
		}

		/// <summary>
		/// Не работает, если нет сеттера ( { set; } )
		/// </summary>
		public static Mailer<TMessage> operator +(Mailer<TMessage> property, Receiver<TMessage> receiver)
		{
			property.Subscribe(receiver);
			return property;
		}

		/// <summary>
		/// Не работает, если нет сеттера ( { set; } )
		/// </summary>
		public static Mailer<TMessage> operator -(Mailer<TMessage> property, Receiver<TMessage> receiver)
		{
			property.Unsubscribe(receiver);
			return property;
		}

		public static Receiver<TMessage> operator +(Receiver<TMessage> receiver, Mailer<TMessage> property)
		{
			property.Subscribe(receiver);
			return receiver;
		}

		public static Receiver<TMessage> operator -(Receiver<TMessage> receiver, Mailer<TMessage> property)
		{
			property.Unsubscribe(receiver);
			return receiver;
		}
	}
}
