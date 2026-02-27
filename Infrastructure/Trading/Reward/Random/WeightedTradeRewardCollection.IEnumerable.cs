using System.Collections.Generic;
using Sapientia;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class WeightedTradeRewardCollection //: IEnumerable<TradeReward>
	{
		protected internal IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			items.Roll<WeightedReward, Blackboard>(board, randomizer, out var index);
			foreach (var reward in items[index].reward.EnumerateActual(board))
				yield return reward;
		}
	}
}
