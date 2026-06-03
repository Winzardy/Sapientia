namespace Trading.Result
{
	/// <inheritdoc cref="ITradeRewardResultHandle"/>
	public abstract class TradeRewardResultHandle<T> : ITradeRewardResultHandle<T>
		where T : TradeReward
	{
		protected T _source;

		TradeReward ITradeRewardResultHandle.Source => _source;

		void ITradeRewardResultHandle<T>.Bind(T source)
		{
			_source = source;
			OnBind();
		}

		public abstract ITradeRewardResult Bake();

		protected virtual void OnBind()
		{
		}

		public void Release()
		{
			OnRelease();
			_source = null;
		}

		protected virtual void OnRelease()
		{
		}
	}
}
