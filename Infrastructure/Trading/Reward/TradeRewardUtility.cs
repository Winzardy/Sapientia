using System.Collections.Generic;
using Content;
using Sapientia.Collections;
using Sapientia.Pooling;
using Sirenix.Utilities;
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
			if (CollectionsExt.IsNullOrEmpty(results))
				yield break;

			using (TradeRewardResultHelper.ForceFullExpansion(forceFullExpansion))
			{
				foreach (var result in results)
				{
					if (result is IEnumerable<ITradeRewardResult> enumerable)
					{
						foreach (var child in ExpandAll(enumerable))
							yield return child;
					}
					else
					{
						yield return result;
					}
				}
			}
		}

		public static IEnumerable<ITradeRewardResult> ExpandAll(
			this IEnumerable<ITradeRewardResult> results)
		{
			if (results.IsNullOrEmpty())
				yield break;

			using (HashSetPool<ITradeRewardResult>.Get(out var visited))
			{
				foreach (var result in ExpandAllInternal(results, visited))
					yield return result;
			}
		}

		private static IEnumerable<ITradeRewardResult> ExpandAllInternal(
			IEnumerable<ITradeRewardResult> results,
			HashSet<ITradeRewardResult> visited)
		{
			foreach (var result in results)
			{
				if (!visited.Add(result))
				{
					yield return result;
					continue;
				}

				if (result is IEnumerable<ITradeRewardResult> nested)
				{
					foreach (var child in ExpandAllInternal(nested, visited))
						yield return child;
				}
				else
				{
					yield return result;
				}
			}
		}

		public static IEnumerable<ITradeRewardResult> MergeAll(this ITradeRewardResult[] results, bool forceFullExpansion = false)
		{
			if (CollectionsExt.IsNullOrEmpty(results))
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
