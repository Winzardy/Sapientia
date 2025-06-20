using System.Collections;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class TradeRewardCollection : IEnumerable<TradeReward>
	{
		public override IEnumerator<TradeReward> GetEnumerator()
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
	}
}
