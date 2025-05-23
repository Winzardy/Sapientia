using System;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	[Serializable]
	public sealed partial class TradeCostCollection : TradeCost
	{
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		// ReSharper disable once UseArrayEmptyMethod
		public TradeCost[] items = new TradeCost[0];

		public TradeCost[] Items => items;

		public override bool CanPay(out TradePayError? error)
		{
			using (ListPool<TradePayError?>.Get(out var errors))
			using (ListPool<TradeCost>.Get(out var sorted))
			{
				sorted.AddRange(items);
				sorted.Sort(SortByPriority);

				error = null;

				foreach (var cost in sorted)
				{
					if (cost.CanPay(out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradePayError(TradeCostCategory.COLLECTION, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		protected override async Task<bool> PayAsync(CancellationToken cancellationToken)
		{
			using (ListPool<TradeCost>.Get(out var paid))
			using (ListPool<TradeCost>.Get(out var sorted))
			{
				try
				{
					sorted.AddRange(items);
					sorted.Sort(SortByPriority);

					foreach (var item in sorted)
					{
						cancellationToken.ThrowIfCancellationRequested();
						var success = await item.ExecuteAsync(cancellationToken);

						if (!success)
						{
							await ReturnAsync(paid);
							return false;
						}

						paid.Add(item);
					}

					return true;
				}
				catch (OperationCanceledException)
				{
					await ReturnAsync(paid);
					throw;
				}
			}
		}

		private int SortByPriority(TradeCost x, TradeCost y) => y.Priority.CompareTo(x.Priority);
	}
}
