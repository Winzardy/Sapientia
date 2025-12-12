namespace Trading
{
	internal interface ITradeResultHandleToken
	{
		public void Release();

		public void ReturnToPool();
	}
}
