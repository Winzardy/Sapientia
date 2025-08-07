using System;
using Sapientia;
using Sapientia.Collections;

namespace Trading.UsagePass
{
	public interface IUsagePassBackend : ITradeReceiptRegistry<UsagePassTradeReceipt>
	{
		public ref readonly UsageLimitModel GetModel(string key);
	}

	[Serializable]
	public class UsagePassTradingModel : IUsagePassBackend
	{
		public HashMap<string, UsageLimitModel> keyToModel;
		public HashMap<string, UsagePassTradeReceipt> keyToReceipt;

		public UsagePassTradingModel()
		{
			keyToModel = new HashMap<string, UsageLimitModel>();
			keyToReceipt = new HashMap<string, UsagePassTradeReceipt>();
		}

		public UsagePassTradingModel(UsagePassTradingModel source)
		{
			keyToModel = new(source.keyToModel);
			keyToReceipt = new(source.keyToReceipt);
		}

		public void Register(string tradeId, in UsagePassTradeReceipt receipt)
		{
			var key = receipt.GetKey(tradeId);
			keyToReceipt.SetOrAdd(key, in receipt);
		}

		public bool CanIssue(Tradeboard board, string key) => keyToReceipt.Contains(key);

		// var cost = board.Get<UsageLimitTradeCost>();
		// ref readonly var model = ref GetModel(key);
		// ref readonly var receipt = ref keyToReceipt.GetOrDefault(key);
		// var dateTime = new DateTime(receipt.timestamp);
		// return cost.entry.CanApplyUsage(in model, dateTime, out _);

		public bool Issue(Tradeboard board, string key)
		{
			var cost = board.Get<UsagePassTradeCost>();
			ref var model = ref keyToModel.GetOrAdd(key);
			ref readonly var receipt = ref keyToReceipt.GetOrDefault(key);
			var dateTime = new DateTime(receipt.timestamp);
			if (board.IsRestored())
				model.TryApplyUsage(in cost.limit, dateTime, out _);
			else
				model.ApplyUsage(in cost.limit, dateTime);
			keyToReceipt.Remove(key);
			return true;
		}

		public ref readonly UsageLimitModel GetModel(string key) => ref keyToModel.GetOrAdd(key);
	}
}
