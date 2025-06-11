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

		protected override async Task<bool> RefundAsync(Tradeboard board, CancellationToken cancellationToken = default)
		{
			var success = true;
			foreach (var cost in items)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await cost.ExecuteRefundAsync(board);
				if (!itemSuccess)
					success = false;
			}

			return success;
		}

		private async Task<bool> RefundAsync(Tradeboard board, List<TradeCost> paid)
		{
			var success = true;
			foreach (var cost in paid)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await cost.ExecuteRefundAsync(board);
				if (!itemSuccess)
					success = false;
			}

			return success;
		}
	}
}
