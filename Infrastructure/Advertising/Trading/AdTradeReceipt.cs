using System;
using Advertising;
using Content;
using Sapientia.Extensions;

namespace Trading.Advertising
{
	[Serializable]
	public struct AdTradeReceipt : ITradeReceipt
	{
		internal const string AD_TOKEN_LABEL_CATALOG = "AdTokenGroup";

		[ContextLabel(AD_TOKEN_LABEL_CATALOG)]
		public int group;

		public AdPlacementType type;
		public string placement;

#if NEWTONSOFT
		[Newtonsoft.Json.JsonIgnore]
#endif
		public string Key => AdTradeReceiptUtility.Combine(group, type, placement);

		public AdTradeReceipt(int group, AdPlacementEntry entry)
		{
			this.group = group;

			type = entry.Type;
			placement = entry.Id;
		}

		void ITradeReceipt.Register(ITradingModel model, string tradeId) => this.Register(model, tradeId);

		public override string ToString() => Key;

		public static AdTradeReceipt Empty(int group, AdPlacementEntry entry) => new()
		{
			group = group,
			type = entry.Type
		};

		bool ITradeReceipt.NeedPush() => !placement.IsNullOrEmpty();
	}

	public static class AdTradeReceiptUtility
	{
		public static string Combine(int group, AdPlacementType type, string id) =>
			"[" + group + "]" + "_"
			+ type + "_"
			+ id;

		public static string ToReceiptId(this AdPlacementEntry entry, int group) => Combine(group, entry.Type, entry.Id);

		public static string ToReceiptId<T>(this ContentReference<T> reference, int group)
			where T : AdPlacementEntry
			=> ToReceiptId(reference.Read(), group);
	}
}
