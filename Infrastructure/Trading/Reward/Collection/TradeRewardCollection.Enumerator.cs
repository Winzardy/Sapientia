using System.Collections.Generic;

namespace Trading
{
	public partial class TradeRewardCollection
	{
		public IEnumerator<TradeReward> GetEnumerator()
		{
			foreach (var item in items)
				yield return item;
		}
	}
}
