using System.Collections.Generic;
using Sapientia;

namespace Trading
{
	public partial class WeightedTradeRewardCollection
	{
		public override IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
			for (var i = 0; i < items.Length; i++)
				yield return items[i].reward;
		}

		protected internal override IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			var evaluatedCount = count.Evaluate(board);
			foreach (var index in items.RollMany<TradeWeightedRewardItem, Blackboard>(board, rollMode, evaluatedCount, randomizer))
			{
				foreach (var reward in items[index].reward.EnumerateActualInternal(board))
					yield return reward;
			}
		}
	}
}
