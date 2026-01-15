namespace Trading.Result
{
	/// <summary>
	/// Запечённое состояние награды, предназначенное для передачи, визуализации и дальнейшего
	/// использования без привязки к жизненному циклу контекста
	/// </summary>
	public interface ITradeRewardResult
	{
		public bool Merge(ITradeRewardResult other) => false;

		public void Return(Tradeboard board);
	}
}
