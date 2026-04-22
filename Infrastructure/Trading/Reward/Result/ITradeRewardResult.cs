namespace Trading.Result
{
	/// <summary>
	/// Запечённое состояние награды, предназначенное для передачи, визуализации и дальнейшего
	/// использования без привязки к жизненному циклу контекста
	/// </summary>
	public interface ITradeRewardResult
	{
		const string FORCE_FULL_EXPANSION_KEY = "forceFullExpansion";

		bool Merge(ITradeRewardResult other) => false;

		void Return(Tradeboard board);
	}
}
