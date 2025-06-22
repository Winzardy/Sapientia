using System;
using System.Collections.Generic;
using Content;
using Sapientia.Extensions;

namespace InAppPurchasing
{
	public class IAPBillingType
	{
		public const string UNDEFINED = "Undefined";

		public const string GOOGLE_PLAY = "Google Play";
		public const string APP_STORE = "App Store";
		public const string AMAZON = "Amazon";
		public const string XSOLLA = "Xsolla";
	}

	[Serializable]
	[Constants(customConstants: new[]
	{
		"Undefined"
	})]
	public partial struct IAPBillingEntry
	{
		public string platform;

		public IAPBillingEntry(string platform) => this.platform = platform;

		public static implicit operator string(IAPBillingEntry billing) => billing.platform;
		public static implicit operator IAPBillingEntry(string platform) => new(platform);

		public static implicit operator bool(IAPBillingEntry billing) =>
			!billing.platform.IsNullOrEmpty() || billing.platform == IAPBillingType.UNDEFINED;

		public override int GetHashCode() => platform?.GetHashCode() ?? 0;

		public override string ToString() => platform ?? IAPBillingType.UNDEFINED;
	}

	//TODO: Важно что выбранный магазин выбирается не от платформы приложения,
	//Важно учитывать что мы можем билдить на андроид для разных магазинов!
	public static class IAPBillingUtility
	{
		public static IEnumerable<IAPBillingEntry> GetAll()
		{
			yield return IAPBillingType.GOOGLE_PLAY;
			yield return IAPBillingType.APP_STORE;
			yield return IAPBillingType.XSOLLA;
			yield return IAPBillingType.AMAZON;
		}

		public static string ToLabel(this in IAPBillingEntry entry) => GetLabel(entry.platform);

		public static string GetLabel(string platform)
		{
			var prefix = platform switch
			{
				IAPBillingType.APP_STORE => "\uf8ff ", //
				IAPBillingType.GOOGLE_PLAY => "\u25B7 ", // ▷
				_ => string.Empty
			};

			return $"{prefix}{platform}";
		}
	}
}
