using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	public partial class TradeRewardCollection
	{
		public override bool CanReturn(out TradeRewardReturnError? error)
		{
			using (ListPool<TradeRewardReturnError?>.Get(out var errors))
			{
				error = null;

				foreach (var item in items)
				{
					if (item.CanReturn(out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeRewardReturnError(TradeRewardReturnCategory.COLLECTION, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		internal override async Task<bool> ReturnAsync(CancellationToken cancellationToken = default)
		{
			var success = true;
			foreach (var reward in items)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await reward.ReturnAsync();
				if (!itemSuccess)
					success = false;
			}

			return success;
		}

		private async Task<bool> ReturnAsync(List<TradeReward> received)
		{
			var success = true;
			foreach (var reward in received)
			{
				// ReSharper disable once MethodSupportsCancellation
				var itemSuccess = await reward.ReturnAsync();
				if (!itemSuccess)
					success = false;
			}

			return success;
		}
	}
}
