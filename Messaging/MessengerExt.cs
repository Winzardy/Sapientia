namespace Sapientia.Messaging
{
	public static class MessengerExt
	{
		public static void Send<TMessage>(this TMessage msg) where TMessage : struct
			=> Messenger.Send(ref msg);

		public static void Send<TMessage>(this TMessage msg, MessengerHub hub) where TMessage : struct
			=> hub.Send(ref msg);
	}
}
