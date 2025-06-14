using System;
using Sapientia.Collections;

namespace Trading.InAppPurchasing
{
	[Serializable]
	public class IAPTradingModel : ITradeReceiptRegistry<IAPTradeReceipt>
	{
		public HashMap<string, IAPTradeModel> tradeToModel;

		public IAPTradingModel()
		{
			tradeToModel = new();
		}

		public IAPTradingModel(IAPTradingModel source)
		{
			tradeToModel = new(source.tradeToModel);

			foreach (var key in tradeToModel.Keys)
			{
				var active = source.tradeToModel[key].active;
				tradeToModel[key].active = new HashMap<string, IAPTradeReceipt>(active);
				var issued = source.tradeToModel[key].issued;
				tradeToModel[key].issued = new HashMap<string, IAPTradeReceipt>(issued);
			}
		}

		public void Register(string tradeId, in IAPTradeReceipt receipt)
		{
			ref var tradeModel = ref tradeToModel.GetOrAdd(tradeId);
			tradeModel.Registry(in receipt);
		}

		public bool CanIssue(Tradeboard board, string key)
		{
			var tradeId = board.Id;
			ref var tradeModel = ref tradeToModel.GetOrAdd(tradeId);
			return tradeModel.CanIssue(key);
		}

		public bool Issue(Tradeboard board, string key)
		{
			var tradeId = board.Id;
			ref var tradeModel = ref tradeToModel.GetOrAdd(tradeId);
			return tradeModel.Issue(key);
		}
	}

	[Serializable]
	public struct IAPTradeModel
	{
		public HashMap<string, IAPTradeReceipt> active;
		public HashMap<string, IAPTradeReceipt> issued;
		// Возможно придется хранить еще и refund...

		public IAPTradeModel(IAPTradeModel source)
		{
			active = new(source.active);
			issued = new(source.issued);
		}
	}
}
