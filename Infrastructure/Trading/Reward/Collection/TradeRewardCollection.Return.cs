using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Trading
{
	public partial class TradeRewardCollection
	{
		protected override bool CanReturn(Tradeboard board, out TradeRewardReturnError? error)
		{
			using (ListPool<TradeRewardReturnError?>.Get(out var errors))
			{
				error = null;

				foreach (var item in items)
				{
					if (item.CanExecuteReturn(board, out error))
						continue;

					errors.Add(error);
				}

				if (!errors.IsEmpty())
					error = new TradeRewardReturnError(ERROR_CATEGORY, errors.ToArray());

				return errors.IsEmpty();
			}
		}

		protected override bool Return(Tradeboard board) => Return(board, items);

		private bool Return(Tradeboard board, IList<TradeReward> received)
		{
			var success = true;
			foreach (var reward in received)
			{
				if (!reward.ExecuteReturn(board))
					success = false;
			}

			return success;
		}
	}
}
