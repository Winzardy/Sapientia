using System;
using System.Collections.Generic;
using Content;
using Sapientia.Extensions;

namespace InAppPurchasing
{
	public class IAPPlatformType
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
	public struct IAPPlatformEntry
	{
		public string platform;

		public IAPPlatformEntry(string platform) => this.platform = platform;

		public static implicit operator string(IAPPlatformEntry platform) => platform.platform;
		public static implicit operator IAPPlatformEntry(string platform) => new(platform);

		public static implicit operator bool(IAPPlatformEntry platform) =>
			!platform.platform.IsNullOrEmpty() || platform.platform == IAPPlatformType.UNDEFINED;

		public override int GetHashCode() => platform?.GetHashCode() ?? 0;

		public override string ToString() => platform ?? IAPPlatformType.UNDEFINED;
	}

	//TODO: Важно что выбранный магазин выбирается не от платформы приложения,
	//Важно учитывать что мы можем билдить на андроид для разных магазинов!
	public static class IAPPlatformExtensions
	{
		public static IEnumerable<IAPPlatformEntry> GetAll()
		{
			yield return IAPPlatformType.GOOGLE_PLAY;
			yield return IAPPlatformType.APP_STORE;
			yield return IAPPlatformType.XSOLLA;
			yield return IAPPlatformType.AMAZON;
		}

		public static string ToLabel(this in IAPPlatformEntry entry) => GetLabel(entry.platform);

		public static string GetLabel(string platform)
		{
			var prefix = platform switch
			{
				IAPPlatformType.APP_STORE => "\uf8ff ", //
				IAPPlatformType.GOOGLE_PLAY => "\u25B7 ", // ▷
				_ => string.Empty
			};

			return $"{prefix}{platform}";
		}
	}
}
