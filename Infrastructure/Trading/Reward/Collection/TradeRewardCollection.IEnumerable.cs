using System.Collections;
using System.Collections.Generic;
using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class TradeRewardCollection : IEnumerable<TradeReward>
	{
		public IEnumerator<TradeReward> GetEnumerator()
		{
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				foreach (var item in sorted)
					foreach (var reward in item)
						yield return reward;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			foreach (var raw in items)
				foreach (var reward in raw.EnumerateActual(board))
					yield return reward;
		}
	}
}
