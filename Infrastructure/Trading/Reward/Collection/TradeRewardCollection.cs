using System;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	[Serializable]
	public partial class TradeRewardCollection : TradeReward
	{
		public const string ERROR_CATEGORY = "Collection";

#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		// ReSharper disable once UseArrayEmptyMethod
		public TradeReward[] items = new TradeReward[0];

		public TradeReward[] Items => items;

		protected internal override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			using (ListPool<TradeReceiveError?>.Get(out var errors))
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				error = null;

				foreach (var cost in sorted)
				{
					if (cost.CanReceive(board, out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeReceiveError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		protected override async Task<bool> ReceiveAsync(Tradeboard board, CancellationToken cancellationToken)
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
						cancellationToken.ThrowIfCancellationRequested();
						var success = await reward.ExecuteAsync(board, cancellationToken);

						if (!success)
						{
							await ReturnAsync(board, received);
							return false;
						}

						received.Add(reward);
					}

					return true;
				}
				catch (OperationCanceledException)
				{
					await ReturnAsync(board, received);
					throw;
				}
			}
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}
}
