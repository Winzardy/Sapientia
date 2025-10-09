using Sapientia;

namespace Trading.UsagePass
{
	public interface IUsagePassNode : ITradeReceiptRegistry<UsagePassTradeReceipt>
	{
		public ref readonly UsageLimitState GetModel(string key);


		public void ForceReset(string key);
	}

	// [Serializable]
	// public class UsagePassTradingModel : IUsagePassNode
	// {
	// 	public HashMap<string, UsageLimitData> keyToModel;
	// 	public HashMap<string, UsagePassTradeReceipt> keyToReceipt;
	//
	// 	public UsagePassTradingModel()
	// 	{
	// 		keyToModel = new HashMap<string, UsageLimitData>();
	// 		keyToReceipt = new HashMap<string, UsagePassTradeReceipt>();
	// 	}
	//
	// 	public UsagePassTradingModel(UsagePassTradingModel source)
	// 	{
	// 		keyToModel = new(source.keyToModel);
	// 		keyToReceipt = new(source.keyToReceipt);
	// 	}
	//
	// 	public void Register(string tradeId, in UsagePassTradeReceipt receipt)
	// 	{
	// 		var key = receipt.GetKey(tradeId);
	// 		keyToReceipt.SetOrAdd(key, in receipt);
	// 	}
	//
	// 	public bool CanIssue(Tradeboard board, string key) => keyToReceipt.Contains(key);
	//
	// 	public bool Issue(Tradeboard board, string key)
	// 	{
	// 		var cost = board.Get<UsagePassTradeCost>();
	// 		ref var model = ref keyToModel.GetOrAdd(key);
	// 		ref readonly var receipt = ref keyToReceipt.GetOrDefault(key);
	// 		var dateTime = new DateTime(receipt.timestamp);
	// 		if (board.IsRestoreState)
	// 			model.TryApplyUsage(in cost.limit, dateTime, out _);
	// 		else
	// 			model.ApplyUsage(in cost.limit, dateTime);
	// 		keyToReceipt.Remove(key);
	// 		return true;
	// 	}
	//
	// 	public ref readonly UsageLimitData GetModel(string key) => ref keyToModel.GetOrAdd(key);
	// 	public void ForceReset(string key)
	// 	{
	// 		if(!keyToModel.Contains(key))
	// 			return;
	// 		ref var model = ref keyToModel[key];
	// 		model.ForceReset();
	// 	}
	// }
}
