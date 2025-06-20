using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Messaging
{
	public sealed partial class MessageBus
	{
		internal static class SubscriptionMap<TMessage> where TMessage : struct
		{
			private static readonly Dictionary<int, Dictionary<IMessageSubscriptionToken, IMessageSubscription>> _hubIndexToGroup = new();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Add(MessageSubscriptionToken<TMessage> token, IMessageSubscription subscription)
			{
				if (!_hubIndexToGroup.TryGetValue(token.HubIndex, out var group))
				{
					group = DictionaryPool<IMessageSubscriptionToken, IMessageSubscription>.Get();
					_hubIndexToGroup[token.HubIndex] = group;
				}

				group.Add(token, subscription);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Remove(MessageSubscriptionToken<TMessage> token)
			{
				var group = _hubIndexToGroup[token.HubIndex];
				group.Remove(token, out _);
				if (group.Count <= 0)
				{
					_hubIndexToGroup.Remove(token.HubIndex);
					group.ReleaseToStaticPool();
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Clear(int hubIndex, bool release = true)
			{
				if (_hubIndexToGroup.TryGetValue(hubIndex, out var tokens))
				{
					tokens.Clear();

					if (release)
					{
						_hubIndexToGroup.Remove(hubIndex);
						tokens.ReleaseToStaticPool();
					}
				}
			}

			/// <summary>
			/// Доставить сообщение по хабу
			/// </summary>
			/// <remarks>
			/// Создаём временный буфер подписок, потому что во время Deliver
			/// список может быть модифицирован (например, через отписку).
			///
			/// GetBusyScope не блокирует текущий поток, если вызов вложенный,
			/// поэтому строго запрещать изменения приведёт к риску взаимоблокировки (deadlock).
			///
			/// Используемая структура SimpleList применяет внутренний пул,
			/// так что лишних аллокаций и утечек памяти не будет
			/// </remarks>
			internal static void Deliver(int hubIndex, ref TMessage msg, bool clear = false)
			{
				if (!_hubIndexToGroup.TryGetValue(hubIndex, out var group))
					return;

				using var subscriptions = new SimpleList<IMessageSubscription>(group.Values);
				if (clear)
					Clear(hubIndex, false);
				foreach (var subscription in subscriptions)
					subscription.Deliver(ref msg);
			}
		}
	}
}
