using System;
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
		public static IEnumerable<TradeReward> EnumerateActual(this TradeReward rootReward, Tradeboard board)
		{
			using (board.SimulationModeScope())
				foreach (var reward in rootReward.EnumerateActualInternal(board))
					yield return reward;
		}

		public static IEnumerable<TradeReward> EnumerateAll(this TradeReward rootReward)
		{
			using (HashSetPool<TradeReward>.Get(out var path))
			{
				foreach (var reward in EnumerateAll(rootReward, path))
					yield return reward;
			}
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

		public static IReadOnlyList<ITradeRewardResult> ExpandAll(this ITradeRewardResult[] results, bool forceFullExpansion = false)
		{
			if (results.IsNullOrEmpty())
				return Array.Empty<ITradeRewardResult>();

			using (Pool<Blackboard>.Get(out var blackboard))
			using (ListPool<ITradeRewardResult>.Get(out var output))
			{
				blackboard.Register(forceFullExpansion, ITradeRewardResult.FORCE_FULL_EXPANSION_KEY);

				foreach (var result in results)
				{
					// дочерние итераторы потребляем жадно прямо здесь, пока доска ещё арендована
					if (result is IEnumerableWithBoard<ITradeRewardResult> enumerableWithBoard)
						output.AddRange(ExpandAll(enumerableWithBoard, blackboard));
					else if (result is IEnumerable<ITradeRewardResult> enumerable)
						output.AddRange(ExpandAll(enumerable, blackboard));
					else
						output.Add(result);
				}

				return output.ToArray();
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

		public static IReadOnlyList<ITradeRewardResult> MergeAll(this ITradeRewardResult[] results, bool forceFullExpansion = false)
		{
			if (results.IsNullOrEmpty())
				return Array.Empty<ITradeRewardResult>();

			using (ListPool<ITradeRewardResult>.Get(out var expanded))
			{
				foreach (var e in results.ExpandAll(forceFullExpansion))
					expanded.Add(e);

				for (int i = 0; i < expanded.Count; i++)
				{
					var target = expanded[i];
					// схлопываем все совместимые с target вправо
					for (int j = i + 1; j < expanded.Count;)
					{
						if (target.Merge(expanded[j]))
							expanded.RemoveAt(j);
						else
							j++;
					}
				}

				return expanded.ToArray();
			}
		}

		private static IEnumerable<TradeReward> EnumerateAll(this TradeReward reward, HashSet<TradeReward> visited)
		{
			if (reward == null)
				yield break;

			if (!visited.Add(reward))
				yield break;

			yield return reward;
			foreach (var child in reward)
			{
				foreach (var nested in EnumerateAll(child, visited))
					yield return nested;
			}
		}
	}
}
