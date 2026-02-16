using System;
using System.Runtime.CompilerServices;

namespace Messaging
{
	public static class MessengerExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Send<TMessage>(this TMessage msg) where TMessage : struct
		{
			Validate<TMessage>();
			return Messenger.Send(ref msg);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TContext, TMessage>(this TContext context, ref TMessage msg, bool replicateWithoutContext = true)
			where TMessage : struct
		{
			Validate<TMessage>();
			Messenger<TContext>.Send(context, ref msg);
			if (replicateWithoutContext)
				msg.Send();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send<TContext, TMessage>(this TContext context, TMessage msg, bool replicateWithoutContext = true)
			where TMessage : struct
		{
			Validate<TMessage>();
			Messenger<TContext>.Send(context, ref msg);
			if (replicateWithoutContext)
				msg.Send();
		}

		private static void Validate<TMessage>() where TMessage : struct
		{
#if DEBUG
			var name = typeof(TMessage).Name;
			if (name.EndsWith("Command"))
			{
				throw new InvalidOperationException($"Invalid message type by name [ {name} ]");
			}
#endif
		}
	}
}
