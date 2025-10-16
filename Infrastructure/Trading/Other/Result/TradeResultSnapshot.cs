using JetBrains.Annotations;
using Sapientia.Collections;
using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
	public struct TradeResultSnapshot
	{
		public string tradeId;

		[CanBeNull]
		public ITradeRewardResult[] rewards;

		[CanBeNull]
		public ITradeCostResult[] costs;

		internal TradeResultSnapshot(string tradeId, TradeRawResult raw)
		{
			this.tradeId = tradeId;

			if (!raw.rewards.IsNullOrEmpty())
			{
				using (ListPool<ITradeRewardResult>.Get(out var r))
				{
					foreach (var handle in raw.rewards)
						r.Add(handle.Bake());

					rewards = r.ToArray();
				}
			}
			else
			{
				rewards = null;
			}

			if (!raw.costs.IsNullOrEmpty())
			{
				using (ListPool<ITradeCostResult>.Get(out var c))
				{
					foreach (var handle in raw.costs)
						c.Add(handle.Bake());

					costs = c.ToArray();
				}
			}
			else
			{
				costs = null;
			}
		}
	}
}
