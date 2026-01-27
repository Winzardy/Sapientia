using System.Collections.Generic;
using Content;
using Sapientia.Collections;
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
			return EnumerateActual(entry.Value, board);
		}

		public static IEnumerable<TradeCost> EnumerateActual(this TradeCost cost, Tradeboard board)
		{
			return cost.EnumerateActual(board);
		}

		public static bool IsEmpty(this ContentEntry<TradeCost> entry, Tradeboard board)
		{
			if (entry.IsEmpty())
				return true;

			return IsEmpty(entry.Value, board);
		}

		public static bool IsEmpty(this TradeCost cost, Tradeboard board)
		{
			if (cost == null)
				return true;

			return cost.EnumerateActual(board)
				.IsNullOrEmpty();
		}
	}
}
