using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	public partial class TradeRewardCollection
	{
		public override bool CanReturn(Tradeboard board, out TradeRewardReturnError? error)
		{
			using (ListPool<TradeRewardReturnError?>.Get(out var errors))
			{
				error = null;

				foreach (var item in items)
				{
					if (item.CanReturn(board, out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeRewardReturnError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		internal override async Task<bool> ReturnAsync(Tradeboard board, CancellationToken cancellationToken = default)
		{
			var success = true;
			foreach (var reward in items)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await reward.ReturnAsync(board);
				if (!itemSuccess)
					success = false;
			}

			return success;
		}

		private async Task<bool> ReturnAsync(Tradeboard board, List<TradeReward> received)
		{
			var success = true;
			foreach (var reward in received)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await reward.ReturnAsync(board);
				if (!itemSuccess)
					success = false;
			}

			return success;
		}
	}
}
