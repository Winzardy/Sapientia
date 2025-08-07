namespace Trading.UsageLimit
{
	public struct UsageLimitTradeReceipt : ITradeReceipt
	{
		/// <summary>
		/// <see cref="DateTime.Ticks"/>
		/// </summary>
		public long timestamp;

		public UsageLimitTradeReceipt(long timestamp)
		{
			this.timestamp = timestamp;
		}

		public readonly string GetKey(string tradeId) => UsageLimitTradeReceiptUtility.ToRecipeKey(tradeId);
	}

	public static class UsageLimitTradeReceiptUtility
	{
		public static string ToRecipeKey(string tradeId) => tradeId;
	}
}
