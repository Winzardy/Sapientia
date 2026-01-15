using System.Collections.Generic;
using Content;
using Trading.Result;

namespace Trading
{
	public static class TradeCostUtility
	{
		public static void RegisterResultHandleTo<TCost, THandle>(this TCost source, Tradeboard board, out THandle handle)
			where TCost : TradeCost
			where THandle : class, ITradeCostResultHandle<TCost>, new()
		{
			handle = RegisterResultHandleTo<TCost, THandle>(source, board);
		}

		public static THandle RegisterResultHandleTo<TCost, THandle>(this TCost source, Tradeboard board)
			where TCost : TradeCost
			where THandle : class, ITradeCostResultHandle<TCost>, new()
		{
			return board.RegisterCostHandle<TCost, THandle>(source);
		}

		/// <inheritdoc cref="TradeCost.EnumerateActual"/>
		public static IEnumerable<TradeCost> EnumerateActual(this ContentEntry<TradeCost> entry, Tradeboard board)
		{
			return entry.Value.EnumerateActual(board);
		}
	}
}
