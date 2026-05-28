using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
	[Flags]
	public enum TradeResultSnapshotFlag
	{
		None   = 0,

		Reward = 1 << 0,
		Cost   = 1 << 1,

		All    = Reward | Cost
	}

	[Serializable]
	public struct TradeResultSnapshot
	{
		public string tradeId;

		[CanBeNull]
		public ITradeRewardResult[] rewards;

		[CanBeNull]
		public ITradeCostResult[] costs;

		public bool IsEmpty { get => rewards.IsNullOrEmpty() && costs.IsNullOrEmpty(); }

		internal TradeResultSnapshot(string tradeId, in TradeRawResult raw, TradeResultSnapshotFlag flags = TradeResultSnapshotFlag.All)
		{
			this.tradeId = tradeId;
			if (flags.HasFlag(TradeResultSnapshotFlag.Reward))
			{
				if (!raw.rewards.IsNullOrEmpty())
				{
					using (ListPool<ITradeRewardResult>.Get(out var bakedRewards))
					{
						foreach (var handle in raw.rewards)
							bakedRewards.Add(handle.Bake());

						rewards = bakedRewards.ToArray();
					}
				}
				else
				{
					rewards = Array.Empty<ITradeRewardResult>();
				}
			}
			else
			{
				rewards = null;
			}

			if (flags.HasFlags(TradeResultSnapshotFlag.Cost))
			{
				if (!raw.costs.IsNullOrEmpty())
				{
					using (ListPool<ITradeCostResult>.Get(out var bakedCosts))
					{
						foreach (var handle in raw.costs)
							bakedCosts.Add(handle.Bake());

						costs = bakedCosts.ToArray();
					}
				}
				else
				{
					costs = Array.Empty<ITradeCostResult>();
				}
			}

			else
			{
				costs = null;
			}
		}
	}

	public static class TradeResultSnapshotUtility
	{
		public static TradeResultSnapshot WithRewards(this in TradeResultSnapshot snapshot, params ITradeRewardResult[] rewards)
		{
			return new TradeResultSnapshot
			{
				tradeId = snapshot.tradeId,

				rewards = rewards,
				costs   = snapshot.costs
			};
		}

		public static TradeResultSnapshot WithCosts(this in TradeResultSnapshot snapshot, params ITradeCostResult[] costs)
		{
			return new TradeResultSnapshot
			{
				tradeId = snapshot.tradeId,

				rewards = snapshot.rewards,
				costs   = costs
			};
		}
	}
}
