namespace Trading.Result
{
	/// <inheritdoc cref="ITradeCostResultHandle"/>
	public abstract class TradeCostResultHandle<TCost> : ITradeCostResultHandle<TCost>
		where TCost : TradeCost
	{
		private TCost _source;

		TradeCost ITradeCostResultHandle.Source => _source;

		public TCost Source => _source;

		void ITradeCostResultHandle<TCost>.Bind(TCost source)
		{
			_source = source;
		}

		public abstract ITradeCostResult Bake();

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
