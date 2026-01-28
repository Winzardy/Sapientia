using System;
using Sapientia.Collections;
using Sapientia.Pooling;
#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public partial class TradeRewardCollection : TradeReward
	{
		public const string ERROR_CATEGORY = "Collection";

		[SerializeReference]
		// ReSharper disable once UseArrayEmptyMethod
		public TradeReward[] items = new TradeReward[0];

		public TradeReward[] Items => items;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			using (ListPool<TradeReceiveError?>.Get(out var errors))
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				error = null;

				foreach (var cost in sorted)
				{
					if (cost.CanExecute(board, out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeReceiveError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		protected override bool Receive(Tradeboard board)
		{
			using (ListPool<TradeReward>.Get(out var received))
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				try
				{
					sorted.AddRange(items);
					sorted.Sort(SortByPriority);

					foreach (var reward in sorted)
					{
						if (!reward.Execute(board))
						{
							board.RefundResult();
							return false;
						}

						received.Add(reward);
					}

					return true;
				}
				catch (Exception e)
				{
					TradingDebug.LogException(e);
					board.RefundResult();
					throw;
				}
			}
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}
}
