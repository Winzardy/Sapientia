using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
	public sealed partial class TradeCostCollection
	{
		public override IEnumerator<TradeCost> GetEnumerator()
		{
			yield return this;
			using (ListPool<TradeCost>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				foreach (var item in sorted)
					yield return item;
			}
		}

		protected internal override IEnumerable<TradeCost> EnumerateActualInternal(Tradeboard board)
		{
			using (ListPool<TradeCost>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				foreach (var cost in sorted)
				{
					foreach (var actualCost in cost.EnumerateActual(board))
						yield return actualCost;
				}
			}
		}
	}
}
