using System.Collections;
using System.Collections.Generic;

namespace Trading
{
	public partial class TradeRewardCollection : IEnumerable<TradeReward>
	{
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<TradeReward> GetEnumerator()
		{
			foreach (var item in items)
				yield return item;
		}
	}
}
