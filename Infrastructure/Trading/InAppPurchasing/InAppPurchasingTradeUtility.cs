using Sapientia.Collections;

namespace Trading.InAppPurchasing
{
	public struct InAppPurchasingTradeData
	{
		public HashMap<string, IAPTradeReceipt> active;
		public HashMap<string, IAPTradeReceipt> issued;
	}

	public static class InAppPurchasingTradeUtility
	{
		public static void Register(this ref InAppPurchasingTradeData data, in IAPTradeReceipt receipt)
		{
			if (data.issued != null && data.issued.Contains(receipt.Id))
			{
				TradingDebug.LogError($"Attempted to register a receipt that has already been issued by id [ {receipt.Id} ]");
				return;
			}

			if (data.active != null && data.active.Contains(receipt.Id))
			{
				TradingDebug.LogError($"Receipt is already registered in active receipts by id [ {receipt.Id} ]");
				return;
			}

			data.active ??= new HashMap<string, IAPTradeReceipt>();
			data.active.SetOrAdd(receipt.Id, in receipt);

			// TODO: IAPManager.Validate(in receipt)
			// Тут надо будет отправить чек на валидацию
		}

		public static bool CanIssue(this ref InAppPurchasingTradeData data, Tradeboard board, string key, out IAPTradeReceipt? tradeReceipt)
		{
			tradeReceipt = null;
			if (data.active.IsNullOrEmpty())
				return false;

			foreach (ref var receipt in data.active)
			{
				if (receipt.GetKey(board.Id) == key)
				{
					tradeReceipt = receipt;
					return true;
				}
			}

			return false;
		}

		public static bool Issue(this ref InAppPurchasingTradeData data, Tradeboard board, string key, out IAPTradeReceipt? tradeReceipt)
		{
			tradeReceipt = null;
			if (data.active.IsNullOrEmpty())
				return false;

			string targetId = null;
			foreach (var id in data.active.Keys)
			{
				if (data.active[id].GetKey(board.Id) != key)
					continue;

				targetId = id;
				break;
			}

			if (targetId == null)
				return false;

			data.issued ??= new HashMap<string, IAPTradeReceipt>();
			ref readonly var activeTradeReceipt = ref data.active[targetId];
			tradeReceipt = activeTradeReceipt;
			data.issued.SetOrAdd(targetId, in activeTradeReceipt);
			data.active.Remove(targetId);
			return true;
		}
	}
}
