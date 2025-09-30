using System.Collections;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class WeightedTradeRewardCollection : IEnumerable<TradeReward>
	{
		public override IEnumerator<TradeReward> GetEnumerator()
		{
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				foreach (var item in items)
					foreach (var reward in item.reward)
						yield return reward;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
