namespace Trading.UsagePass
{
	public struct UsagePassTradeReceipt : ITradeReceipt
	{
		/// <summary>
		/// <see cref="DateTime.Ticks"/>
		/// </summary>
		public long timestamp;

		public UsagePassTradeReceipt(long timestamp)
		{
			this.timestamp = timestamp;
		}

		public readonly string GetKey(string tradeId) => UsagePassTradeReceiptUtility.ToRecipeKey(tradeId);
	}

	public static class UsagePassTradeReceiptUtility
	{
		public static string ToRecipeKey(string tradeId) => tradeId;
	}
}
