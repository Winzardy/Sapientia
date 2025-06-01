using System.Collections.Generic;

namespace Trading
{
	public sealed partial class TradeCostCollection
	{
		public IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var item in items)
				yield return item;
		}
	}
}
