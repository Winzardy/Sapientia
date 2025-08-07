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

		/// <summary>
		/// <see cref="DateTime.Ticks"/>
		/// </summary>
		public long timestamp;

		/// <param name="timestamp"><see cref="DateTime.Ticks"/></param>
		public AdTradeReceipt(int group, AdPlacementEntry entry, long timestamp)
		{
			this.group = group;
			this.timestamp = timestamp;

			type = entry.Type;
			placement = entry.Id;
		}

		public override string ToString()
		{
			return "Ad Trade Receipt:\n" +
				$" Key: {GetKey(null)}\n" +
				$" Type: {type}\n" +
				$" Placement Id: {placement}\n" +
				$" Group: {group}\n";
		}

		public static AdTradeReceipt Empty(int group, AdPlacementEntry entry) => new()
		{
			group = group,
			type = entry.Type
		};

		bool ITradeReceipt.NeedPush() => !placement.IsNullOrEmpty();

		public string GetKey(string _) => AdTradeReceiptUtility.Combine(group, type, placement);
	}

	public static class AdTradeReceiptUtility
	{
		public static string Combine(int group, AdPlacementType type, string id) =>
			"[" + group + "] "
			+ type + "_"
			+ id;

		public static string ToReceiptKey(this AdPlacementEntry entry, int group) => Combine(group, entry.Type, entry.Id);

		public static string ToReceiptKey<T>(this ContentReference<T> reference, int group)
			where T : AdPlacementEntry
			=> ToReceiptKey(reference.Read(), group);
	}
}
