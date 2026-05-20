using System.Collections.Generic;

namespace Trading
{
	public sealed partial class TradeCostOptions
	{
		public override IEnumerator<TradeCost> GetEnumerator()
		{
			yield return this;
			foreach (var option in options)
				yield return option;
		}

		protected internal override IEnumerable<TradeCost> EnumerateActualInternal(Tradeboard board)
		{
			foreach (var actualCost in options[selectedIndex].EnumerateActual(board))
				yield return actualCost;
		}
	}
}
