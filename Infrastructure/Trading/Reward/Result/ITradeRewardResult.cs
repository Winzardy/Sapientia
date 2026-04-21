using System;

namespace Trading.Result
{
	/// <summary>
	/// Запечённое состояние награды, предназначенное для передачи, визуализации и дальнейшего
	/// использования без привязки к жизненному циклу контекста
	/// </summary>
	public interface ITradeRewardResult
	{
		bool Merge(ITradeRewardResult other) => false;

		void Return(Tradeboard board);
	}

	public static class TradeRewardResultHelper
	{
		public static bool forceFullExpansion;

		public static TradeRewardResultForceFullExpansionScope ForceFullExpansion(bool forceFullExpansion)
		{
			return new TradeRewardResultForceFullExpansionScope(forceFullExpansion);
		}
	}

	public readonly struct TradeRewardResultForceFullExpansionScope : IDisposable
	{
		private readonly bool _originForceFullExpansion;

		public TradeRewardResultForceFullExpansionScope(bool forceFullExpansion)
		{
			_originForceFullExpansion = TradeRewardResultHelper.forceFullExpansion;
			TradeRewardResultHelper.forceFullExpansion = forceFullExpansion;
		}

		public void Dispose()
		{
			TradeRewardResultHelper.forceFullExpansion = _originForceFullExpansion;
		}
	}
}
