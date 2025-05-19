using System.Runtime.CompilerServices;

namespace Sapientia.Messaging
{
	public static class MessengerExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TMessage>(this TMessage msg) where TMessage : struct
			=> Messenger.Send(ref msg);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TContext, TMessage>(this TContext context, ref TMessage msg, bool replicateWithoutContext = true)
			where TMessage : struct
		{
			Messenger<TContext>.Send(context, ref msg);
			if (replicateWithoutContext)
				msg.Send();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TContext, TMessage>(this TContext context, TMessage msg, bool replicateWithoutContext = true)
			where TMessage : struct
		{
			Messenger<TContext>.Send(context, ref msg);
			if (replicateWithoutContext)
				msg.Send();
		}
	}
}
