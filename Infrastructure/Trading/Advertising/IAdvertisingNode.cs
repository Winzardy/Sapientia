namespace Trading.Advertising
{
	public interface IAdvertisingNode : ITradeReceiptRegistry<AdTradeReceipt>
	{
		public int GetTokenCount(int group);

		public void AddToken(int group, int count);
	}

}
