using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	public sealed partial class TradeCostCollection
	{
		public override bool CanReturn(out TradeCostReturnError? error)
		{
			using (ListPool<TradeCostReturnError?>.Get(out var errors))
			{
				error = null;

				foreach (var item in items)
				{
					if (item.CanReturn(out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeCostReturnError(TradeCostReturnCategory.COLLECTION, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		internal override async Task<bool> ReturnAsync(CancellationToken cancellationToken = default)
		{
			var success = true;
			foreach (var cost in items)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await cost.ReturnAsync();
				if (!itemSuccess)
					success = false;
			}

			return success;
		}

		private async Task<bool> ReturnAsync(List<TradeCost> paid)
		{
			var success = true;
			foreach (var cost in paid)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await cost.ReturnAsync();
				if (!itemSuccess)
					success = false;
			}

			return success;
		}
	}
}
