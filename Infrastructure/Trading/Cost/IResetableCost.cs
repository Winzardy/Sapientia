using UnityEngine;

namespace Trading
{
	public interface IResettableCost
	{
		public void Reset(Tradeboard board);
	}

	public static class ResettableTradeUtility
	{
		public static void Reset(this TradeCost cost, Tradeboard tradeboard)
		{
			foreach (var x in cost)
			{
				if (x is IResettableCost resettableCost)
					resettableCost.Reset(tradeboard);
			}
		}
	}
}
