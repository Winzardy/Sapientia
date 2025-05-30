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
		public string test;
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		// ReSharper disable once UseArrayEmptyMethod
		public TradeReward[] items = new TradeReward[0];

		public TradeReward[] Items => items;

		public override bool CanReceive(out TradeReceiveError? error)
		{
			using (ListPool<TradeReceiveError?>.Get(out var errors))
			using (ListPool<TradeReward>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				error = null;

				foreach (var cost in sorted)
				{
					if (cost.CanReceive(out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeReceiveError(TradeRewardCategory.COLLECTION, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		internal override async Task<bool> ReceiveAsync(CancellationToken cancellationToken)
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
						var success = await reward.ReceiveAsync(cancellationToken);

						if (!success)
						{
							await ReturnAsync(received);
							return false;
						}

						received.Add(reward);
					}

					return true;
				}
				catch (OperationCanceledException)
				{
					await ReturnAsync(received);
					throw;
				}
			}
		}

		private int SortByPriority(TradeReward x, TradeReward y) => y.Priority.CompareTo(x.Priority);
	}
}
