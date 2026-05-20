using System.Collections.Generic;
using Content;
using Sapientia.Collections;
using Sapientia.Pooling;
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

		/// <inheritdoc cref="TradeCost.EnumerateActualInternal"/>
		public static IEnumerable<TradeCost> EnumerateActual(this ContentEntry<TradeCost> entry, Tradeboard board)
		{
			return EnumerateActual(entry.Value, board);
		}

		public static IEnumerable<TradeCost> EnumerateActual(this TradeCost cost, Tradeboard board)
		{
			if (!board.IsSimulationMode)
				throw TradingDebug.Exception($"Actual trade costs can only be enumerated in simulation mode (board [ {board.Id} ])");

			return cost?.EnumerateActualInternal(board);
		}

		public static IEnumerable<TradeCost> EnumerateAll(this ContentEntry<TradeCost> entry)
		{
			if(entry.IsEmpty())
				yield break;

			foreach (var cost in entry.Value.EnumerateAll())
				yield return cost;
		}

		public static IEnumerable<TradeCost> EnumerateAll(this TradeCost rootCost)
		{
			using (HashSetPool<TradeCost>.Get(out var path))
			{
				foreach (var cost in EnumerateAll(rootCost, path))
					yield return cost;
			}
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

			return EnumerateActual(cost, board)
				.IsNullOrEmpty();
		}

		private static IEnumerable<TradeCost> EnumerateAll(this TradeCost cost, HashSet<TradeCost> visited)
		{
			if (cost == null)
				yield break;

			if (!visited.Add(cost))
				yield break;

			yield return cost;
			foreach (var child in cost)
			{
				foreach (var nested in EnumerateAll(child, visited))
					yield return nested;
			}
		}
	}
}
