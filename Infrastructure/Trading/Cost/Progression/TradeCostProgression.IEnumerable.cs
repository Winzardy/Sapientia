using System.Collections.Generic;

namespace Trading
{
	public partial class TradeCostProgression
	{
		protected internal override IEnumerable<TradeCost> EnumerateActualInternal(Tradeboard board)
		{
			var stage = GetCurrentStage(board);
			foreach (var cost in stage.cost.EnumerateActual(board))
				yield return cost;
		}

		public override IEnumerator<TradeCost> GetEnumerator()
		{
			yield return this;
			for (int i = 0; i < stages.Length; i++)
				yield return stages[i].cost;
		}
	}
}
