using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
	public partial class TradeRewardCollection
	{
		protected internal override IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				foreach (var raw in sorted)
				{
					if (raw == null)
					{
						TradingDebug.LogError($"Null reward in collection (tradeId: {board.Id})");
						continue;
					}

					foreach (var reward in raw.EnumerateActualInternal(board))
						yield return reward;
				}
			}
		}

		public override IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);
				foreach (var reward in sorted)
					yield return reward;
			}
		}
	}
}
