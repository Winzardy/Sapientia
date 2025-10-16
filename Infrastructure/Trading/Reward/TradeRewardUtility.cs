using System.Collections.Generic;
using Content;
using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
	public static class TradeRewardUtility
	{
		/// <inheritdoc cref="EnumerateActual(TradeReward, Tradeboard)"/>
		public static IEnumerable<TradeReward> EnumerateActual(this ContentReference<TradeReward> rewardRef, Tradeboard board)
			=> EnumerateActual(rewardRef.Read(), board);

		/// <summary>
		/// Результативный список наград который в итоге получается при текущем состоянии (на момент запроса)
		/// </summary>
		public static IEnumerable<TradeReward> EnumerateActual(this TradeReward reward, Tradeboard board)
		{
			using (board.DummyModeScope())
				foreach (var actualReward in reward.EnumerateActual(board))
					yield return actualReward;
		}

		public static TradeReward[] GetAllRawReward(this TradeReward reward)
		{
			using (ListPool<TradeReward>.Get(out var list))
			{
				foreach (var r in reward)
				{
					list.Add(r);
				}

				return list.ToArray();
			}
		}

		public static void RegisterResultHandleTo<THandle, TReward>(this TReward source, Tradeboard board, out THandle handle)
			where TReward : TradeReward
			where THandle : class, ITradeRewardResultHandle<TReward>, new()
		{
			handle = RegisterResultHandleTo<THandle, TReward>(source, board);
		}

		public static THandle RegisterResultHandleTo<THandle, TReward>(this TReward source, Tradeboard board)
			where TReward : TradeReward
			where THandle : class, ITradeRewardResultHandle<TReward>, new()
		{
			return board.RegisterRewardHandle<TReward, THandle>(source);
		}
	}
}
