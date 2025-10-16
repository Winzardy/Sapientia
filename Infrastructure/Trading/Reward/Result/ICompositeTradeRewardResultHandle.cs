namespace Trading.Result
{
	public interface ICompositeTradeRewardResult : ITradeRewardResult
	{
		public ITradeRewardResult[] Children { get; }
	}
	public interface ICompositeTradeRewardResultHandle : ITradeRewardResultHandle
	{
		public ITradeRewardResultHandle[] Children { get; }
	}
}
