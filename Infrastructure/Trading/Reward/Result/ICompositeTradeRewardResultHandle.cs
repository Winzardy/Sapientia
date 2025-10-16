namespace Trading.Result
{
	public interface ICompositeTradeRewardResultHandle : ITradeRewardResultHandle
	{
		public ITradeRewardResultHandle[] Children { get; }
	}
}
