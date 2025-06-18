using System;
using Advertising;
using Content;

namespace Trading.Advertising
{
	[Serializable]
	public struct AdTradeReceipt : ITradeReceipt
	{
		public AdPlacementType type;
		public string placement;

#if NEWTONSOFT
		[Newtonsoft.Json.JsonIgnore]
#endif
		public string Key => AdTradeReceiptUtility.Combine(type, placement);

		public AdTradeReceipt(AdPlacementEntry entry)
		{
			type = entry.Type;
			placement = entry.Id;
		}

		void ITradeReceipt.Registry(ITradingModel model, string tradeId) => this.Registry(model, tradeId);

		public override string ToString() => $"Ad Receipt: {Key}";
	}

	public static class AdTradeReceiptUtility
	{
		public static string Combine(AdPlacementType type, string id) => type + "+" + id;

		public static string ToReceiptId(this AdPlacementEntry entry) => Combine(entry.Type, entry.Id);

		public static string ToReceiptId<T>(this ContentReference<T> reference)
			where T : AdPlacementEntry
			=> ToReceiptId(reference.Read());
	}
}
