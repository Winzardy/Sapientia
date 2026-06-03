namespace Trading
{
	public static class ResettableTradeUtility
	{
		public static void Reset(this TradeCost cost, Tradeboard tradeboard)
		{
			foreach (var x in cost)
			{
				if (x is ITradeResettable resettableCost)
					resettableCost.Reset(tradeboard);
			}
		}

		public static void Reset(this TradeReward reward, Tradeboard tradeboard)
		{
			foreach (var x in reward)
			{
				if (x is ITradeResettable resettableCost)
					resettableCost.Reset(tradeboard);
			}
		}
	}

	public interface ITradeResettable
	{
		void Reset(Tradeboard board);
	}
}
