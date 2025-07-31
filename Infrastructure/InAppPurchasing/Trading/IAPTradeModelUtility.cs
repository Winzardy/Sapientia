using Sapientia.Collections;

namespace Trading.InAppPurchasing
{
	public static class IAPTradeModelUtility
	{
		public static void Register(this ref IAPTradeModel model, in IAPTradeReceipt receipt)
		{
			if (model.issued != null && model.issued.Contains(receipt.Id))
			{
				TradingDebug.LogError($"Attempted to register a receipt that has already been issued by id [ {receipt.Id} ]");
				return;
			}

			if (model.issued != null && model.active.Contains(receipt.Id))
			{
				TradingDebug.LogError($"Receipt is already registered in active receipts by id [ {receipt.Id} ]");
				return;
			}

			model.active ??= new HashMap<string, IAPTradeReceipt>();
			model.active.SetOrAdd(receipt.Id, in receipt);

			// IAPManager.Validate()
			// Тут надо будет отправить чек на валидацию
		}

		public static bool CanIssue(this ref IAPTradeModel model, string key)
		{
			if (model.active.IsNullOrEmpty())
				return false;

			foreach (ref var receipt in model.active)
			{
				if (receipt.Key == key)
					return true;
			}

			return false;
		}

		public static bool Issue(this ref IAPTradeModel model, string key)
		{
			if (model.active.IsNullOrEmpty())
				return false;

			string targetId = null;
			foreach (var id in model.active.Keys)
			{
				if (model.active[id].Key != key)
					continue;

				targetId = id;
				break;
			}

			if (targetId == null)
				return false;

			model.issued ??= new HashMap<string, IAPTradeReceipt>();
			model.issued.SetOrAdd(targetId, in model.active[targetId]);
			model.active.Remove(targetId);
			return true;
		}
	}
}
