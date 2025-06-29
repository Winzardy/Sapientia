using System;
using Advertising;
using Content;
using Sapientia.Extensions;

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

		void ITradeReceipt.Register(ITradingModel model, string tradeId) => this.Register(model, tradeId);

		public override string ToString() => $"Ad Receipt: {Key}";

		public static AdTradeReceipt Empty(AdPlacementEntry entry) => new()
			{
				type = entry.Type
			};

		bool ITradeReceipt.NeedPush() => !placement.IsNullOrEmpty();
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
