using System;
using Sapientia;
using Sapientia.Collections;

namespace Trading.UsageLimit
{
	public interface IUsageLimitBackend : ITradeReceiptRegistry<UsageLimitTradeReceipt>
	{
		public ref readonly UsageLimitModel GetModel(string key);
	}

	[Serializable]
	public class UsageLimitTradingModel : IUsageLimitBackend
	{
		public HashMap<string, UsageLimitModel> keyToModel;
		public HashMap<string, UsageLimitTradeReceipt> keyToReceipt;

		public UsageLimitTradingModel()
		{
			keyToModel = new HashMap<string, UsageLimitModel>();
			keyToReceipt = new HashMap<string, UsageLimitTradeReceipt>();
		}

		public UsageLimitTradingModel(UsageLimitTradingModel source)
		{
			keyToModel = new(source.keyToModel);
			keyToReceipt = new(source.keyToReceipt);
		}

		public void Register(string tradeId, in UsageLimitTradeReceipt receipt)
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
			var cost = board.Get<UsageLimitTradeCost>();
			ref var model = ref keyToModel.GetOrAdd(key);
			ref readonly var receipt = ref keyToReceipt.GetOrDefault(key);
			var dateTime = new DateTime(receipt.timestamp);
			if (board.IsRestored())
				model.TryApplyUsage(in cost.usageLimit, dateTime, out _);
			else
				model.ApplyUsage(in cost.usageLimit, dateTime);
			keyToReceipt.Remove(key);
			return true;
		}

		public ref readonly UsageLimitModel GetModel(string key) => ref keyToModel.GetOrAdd(key);
	}
}
