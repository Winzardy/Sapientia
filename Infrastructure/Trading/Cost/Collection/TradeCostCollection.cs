using System;
using Sapientia.Collections;
using Sapientia.Pooling;
#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	[Serializable]
	public sealed partial class TradeCostCollection : TradeCost
	{
		public const string ERROR_CATEGORY = "Collection";

		[SerializeReference]
		[TradeAccess(TradeAccessType.ByParent)]
		// ReSharper disable once UseArrayEmptyMethod
		// ReSharper disable once MemberInitializerValueIgnored
		public TradeCost[] items = new TradeCost[0];

		public TradeCost[] Items => items;

		public TradeCostCollection(TradeCost[] items)
		{
			this.items = items;
		}

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			using (ListPool<TradePayError?>.Get(out var errors))
			using (ListPool<TradeCost>.Get(out var sorted))
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
					error = new TradePayError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		protected override bool Pay(Tradeboard board)
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
						if (!item.Execute(board))
						{
							Refund(board, paid);
							return false;
						}

						paid.Add(item);
					}

					return true;
				}
				catch (Exception e)
				{
					TradingDebug.LogException(e);
					Refund(board, paid);
					throw;
				}
			}
		}

		private int SortByPriority(TradeCost x, TradeCost y) => y.Priority.CompareTo(x.Priority);
	}
}
