using System.Runtime.CompilerServices;

namespace Sapientia.Messaging
{
	public static class MessengerExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SendAndUnsubscribeAll<TMessage>(this TMessage msg) where TMessage : struct
			=> Messenger.SendSendAndUnsubscribeAll(ref msg);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TMessage>(this TMessage msg) where TMessage : struct
			=> Messenger.Send(ref msg);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TMessage>(this TMessage msg, MessengerHub hub) where TMessage : struct
			=> hub.Send(ref msg);
	}
}
