using System.Collections;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public sealed partial class TradeCostCollection : IEnumerable<TradeCost>
	{
		public override IEnumerator<TradeCost> GetEnumerator()
		{
			using (ListPool<TradeCost>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				foreach (var item in sorted)
					foreach (var cost in item) // ограничение на одну вложенность в системе!
						yield return cost;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected internal  override IEnumerable<TradeCost> EnumerateActualInternal(Tradeboard board)
		{
			foreach (var cost in items)
				foreach (var actualCost in cost.EnumerateActual(board))
					yield return actualCost;
		}
	}
}
