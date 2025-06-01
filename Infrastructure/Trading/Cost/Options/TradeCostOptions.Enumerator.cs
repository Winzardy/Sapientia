using System.Collections.Generic;

namespace Trading
{
	public sealed partial class TradeCostOptions
	{
		public IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var option in options)
				yield return option;
		}
	}
}
