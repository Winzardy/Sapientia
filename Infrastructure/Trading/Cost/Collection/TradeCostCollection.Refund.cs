using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	public sealed partial class TradeCostCollection
	{
		protected override bool CanRefund(Tradeboard board, out TradeCostRefundError? error)
		{
			using (ListPool<TradeCostRefundError?>.Get(out var errors))
			{
				error = null;

				foreach (var item in items)
				{
					if (item.CanExecuteRefund(board, out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeCostRefundError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		protected override bool Refund(Tradeboard board)
		{
			var success = true;
			foreach (var cost in items)
			{
				if (!cost.ExecuteRefund(board))
					success = false;
			}

			return success;
		}

		private bool Refund(Tradeboard board, List<TradeCost> paid)
		{
			var success = true;
			foreach (var cost in paid)
			{
				if (!cost.ExecuteRefund(board))
					success = false;
			}

			return success;
		}
	}
}
