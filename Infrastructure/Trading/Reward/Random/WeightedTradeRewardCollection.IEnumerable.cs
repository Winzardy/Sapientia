using System.Collections;
using System.Collections.Generic;
using Sapientia;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class WeightedTradeRewardCollection : IEnumerable<TradeReward>
	{
		public override IEnumerator<TradeReward> GetEnumerator()
		{
			foreach (var item in items)
				foreach (var reward in item.reward)
					yield return reward;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected internal override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			var randomizer = board.Get<IRandomizer<int>>();
			items.Roll(randomizer, out var index);
			foreach (var reward in items[index].reward.EnumerateActual(board))
				yield return reward;
		}
	}
}
