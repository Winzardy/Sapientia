namespace Trading
{
	public static class ResettableTradeUtility
	{
		public static void Reset(this TradeCost cost, Tradeboard tradeboard)
		{
			// Перебор самой цены раскрывает только один уровень, а сбросить надо всю вложенность
			foreach (var x in cost.EnumerateAll())
			{
				if (x is ITradeResettable resettableCost)
					resettableCost.Reset(tradeboard);
			}
		}

		public static void Reset(this TradeReward reward, Tradeboard tradeboard)
		{
			foreach (var x in reward.EnumerateAll())
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
