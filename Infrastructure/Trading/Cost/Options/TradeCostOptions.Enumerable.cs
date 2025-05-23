using System.Collections;
using System.Collections.Generic;

namespace Trading
{
	public sealed partial class TradeCostOptions : IEnumerable<TradeCost>
	{
		public IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var option in options)
				yield return option;
		}

		IEnumerator IEnumerable.GetEnumerator() => options.GetEnumerator();
	}
}
