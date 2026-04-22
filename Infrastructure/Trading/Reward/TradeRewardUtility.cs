using System.Collections.Generic;
using Content;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
	public static class TradeRewardUtility
	{
		/// <inheritdoc cref="EnumerateActual(TradeReward, Tradeboard)"/>
		public static IEnumerable<TradeReward> EnumerateActual(this ContentReference<TradeReward> rewardRef, Tradeboard board)
			=> EnumerateActual(rewardRef.Read(), board);

		/// <summary>
		/// Результативный список наград который в итоге получается при текущем состоянии (на момент запроса)
		/// </summary>
		public static IEnumerable<TradeReward> EnumerateActual(this TradeReward reward, Tradeboard board)
		{
			using (board.SimulationModeScope())
				foreach (var actualReward in reward.EnumerateActualInternal(board))
					yield return actualReward;
		}

		public static void RegisterResultHandleTo<THandle, TReward>(this TReward source, Tradeboard board, out THandle handle)
			where TReward : TradeReward
			where THandle : class, ITradeRewardResultHandle<TReward>, new()
		{
			handle = RegisterResultHandleTo<THandle, TReward>(source, board);
		}

		public static THandle RegisterResultHandleTo<THandle, TReward>(this TReward source, Tradeboard board)
			where TReward : TradeReward
			where THandle : class, ITradeRewardResultHandle<TReward>, new()
		{
			return board.RegisterRewardHandle<TReward, THandle>(source);
		}

		public static IEnumerable<ITradeRewardResult> ExpandAll(this ITradeRewardResult[] results, bool forceFullExpansion = false)
		{
			if (results.IsNullOrEmpty())
				yield break;

			using (Pool<Blackboard>.Get(out var blackboard))
			{
				blackboard.Register(forceFullExpansion, ITradeRewardResult.FORCE_FULL_EXPANSION_KEY);
				foreach (var result in results)
				{
					if (result is IEnumerableWithBoard<ITradeRewardResult> enumerableWithBoard)
					{
						foreach (var child in ExpandAll(enumerableWithBoard, blackboard))
							yield return child;
					}
					else if (result is IEnumerable<ITradeRewardResult> enumerable)
					{
						foreach (var child in ExpandAll(enumerable, blackboard))
							yield return child;
					}
					else
					{
						yield return result;
					}
				}
			}
		}

		private static IEnumerable<ITradeRewardResult> ExpandAll(this IEnumerableWithBoard<ITradeRewardResult> results, Blackboard board)
		{
			using (HashSetPool<ITradeRewardResult>.Get(out var visited))
			{
				using var enumerator = results.GetEnumerator(board);
				while (enumerator.MoveNext())
				{
					foreach (var result in ExpandAllInternal(enumerator.Current, board, visited))
						yield return result;
				}
			}
		}

		private static IEnumerable<ITradeRewardResult> ExpandAll(this IEnumerable<ITradeRewardResult> results, Blackboard board)
		{
			if (results.IsNullOrEmpty())
				yield break;

			using (HashSetPool<ITradeRewardResult>.Get(out var visited))
			{
				foreach (var result in ExpandAllInternal(results, board, visited))
					yield return result;
			}
		}

		private static IEnumerable<ITradeRewardResult> ExpandAllInternal(
			IEnumerable<ITradeRewardResult> results,
			Blackboard board,
			HashSet<ITradeRewardResult> visited)
		{
			foreach (var result in results)
			{
				foreach (var child in ExpandAllInternal(result, board, visited))
					yield return child;
			}
		}

		private static IEnumerable<ITradeRewardResult> ExpandAllInternal(
			ITradeRewardResult result,
			Blackboard board,
			HashSet<ITradeRewardResult> visited)
		{
			if (!visited.Add(result))
			{
				yield return result;
				yield break;
			}

			if (result is IEnumerableWithBoard<ITradeRewardResult> nestedWithBoard)
			{
				using var enumerator = nestedWithBoard.GetEnumerator(board);
				while (enumerator.MoveNext())
				{
					foreach (var child in ExpandAllInternal(enumerator.Current, board, visited))
						yield return child;
				}
			}
			else if (result is IEnumerable<ITradeRewardResult> nested)
			{
				foreach (var child in ExpandAllInternal(nested, board, visited))
					yield return child;
			}
			else
			{
				yield return result;
			}
		}

		public static IEnumerable<ITradeRewardResult> MergeAll(this ITradeRewardResult[] results, bool forceFullExpansion = false)
		{
			if (results.IsNullOrEmpty())
				yield break;

			using (ListPool<ITradeRewardResult>.Get(out var expanded))
			{
				foreach (var e in results.ExpandAll(forceFullExpansion))
					expanded.Add(e);

				if (expanded.Count == 0)
					yield break;

				for (int i = 0; i < expanded.Count; i++)
				{
					var target = expanded[i];
					for (int j = i + 1; j < expanded.Count;)
					{
						var candidate = expanded[j];
						if (target.Merge(candidate))
						{
							expanded.RemoveAt(j);
						}
						else
						{
							j++;
						}
					}
				}

				for (int k = 0; k < expanded.Count; k++)
					yield return expanded[k];
			}
		}
	}
}
