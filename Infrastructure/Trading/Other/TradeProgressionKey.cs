using System;
using Content;
#if CLIENT
using Sirenix.OdinInspector;
#endif

namespace Trading
{
	[Serializable]
#if CLIENT
	[HideLabel]
#endif
	public struct TradeProgressionKey
	{
#if CLIENT
		[LabelText("Source")]
#endif
		public TradeProgressionSchemeSource schemeSource;

#if CLIENT
		[LabelText("Reference")]
		[ShowIf(nameof(schemeSource), TradeProgressionSchemeSource.Shared)]
#endif
		public ContentReference<TradeProgressionScheme> schemeRef;

#if CLIENT

		[ShowIf(nameof(schemeSource), TradeProgressionSchemeSource.Local)]
#endif
		public SerializableGuid key;

		public string ToKey() => schemeSource == TradeProgressionSchemeSource.Shared ? schemeRef.guid : key;

		public TradeProgressionScheme ResolveScheme()
		{
			var guid = schemeSource == TradeProgressionSchemeSource.Shared ? schemeRef.guid : key;
			return ContentManager.Get<TradeProgressionScheme>(guid);
		}

		public static implicit operator string(in TradeProgressionKey key) => key.ToKey();
		public static implicit operator TradeProgressionScheme(in TradeProgressionKey key) => key.ResolveScheme();
	}

	public static class TradeProgressionKeyExtensions
	{
		public static int GetCurrentProgress(this ITradingNode node, TradeProgressionKey key) =>
			node.GetCurrentProgress(key, key.ResolveScheme());

		public static void IncrementProgress(this ITradingNode node, TradeProgressionKey key) =>
			node.IncrementProgress(key, key.ResolveScheme());

		public static void ChangeProgress(this ITradingNode node, TradeProgressionKey key, int value) =>
			node.ChangeProgress(key, value, key.ResolveScheme());

		public static void ResetProgress(this ITradingNode node, TradeProgressionKey key) =>
			node.ResetProgress(key, key.ResolveScheme());
	}
}
