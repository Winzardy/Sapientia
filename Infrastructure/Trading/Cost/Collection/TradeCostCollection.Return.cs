using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	public sealed partial class TradeCostCollection
	{
		public override bool CanReturn(Tradeboard board, out TradeCostReturnError? error)
		{
			using (ListPool<TradeCostReturnError?>.Get(out var errors))
			{
				error = null;

				foreach (var item in items)
				{
					if (item.CanReturn(board, out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeCostReturnError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		internal override async Task<bool> ReturnAsync(Tradeboard board, CancellationToken cancellationToken = default)
		{
			var success = true;
			foreach (var cost in items)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await cost.ReturnAsync(board);
				if (!itemSuccess)
					success = false;
			}

			return success;
		}

		private async Task<bool> ReturnAsync(Tradeboard board, List<TradeCost> paid)
		{
			var success = true;
			foreach (var cost in paid)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await cost.ReturnAsync(board);
				if (!itemSuccess)
					success = false;
			}

			return success;
		}
	}
}
