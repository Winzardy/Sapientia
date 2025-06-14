using System;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Targeting
{
	public class StorePlatformType
	{
		public const string UNDEFINED = "Undefined";

		public const string GOOGLE_PLAY = "Google Play";
		public const string APP_STORE = "App Store";
		public const string AMAZON = "Amazon";
	}

	/// <summary>
	/// Это платформа магазина на которой расспротраняется приложение (Distribution)!
	/// </summary>
	[Serializable]
	public partial struct StorePlatformEntry
	{
		public string store;

		public StorePlatformEntry(string store) => this.store = store;

		public static implicit operator string(StorePlatformEntry platform) => platform.store;
		public static implicit operator StorePlatformEntry(string name) => new(name);

		public static implicit operator bool(StorePlatformEntry platform) =>
			!platform.store.IsNullOrEmpty() || platform.store == StorePlatformType.UNDEFINED;

		public override int GetHashCode() => store?.GetHashCode() ?? 0;

		public override string ToString() => store ?? StorePlatformType.UNDEFINED;
	}

	public static class StorePlatformUtility
	{
		public static IEnumerable<StorePlatformEntry> GetAllPlatforms()
		{
			yield return StorePlatformType.GOOGLE_PLAY;
			yield return StorePlatformType.APP_STORE;
			yield return StorePlatformType.AMAZON;
		}

		public static string ToLabel(this in StorePlatformEntry entry) => GetLabel(entry.store);

		public static string GetLabel(string platform)
		{
			var prefix = platform switch
			{
				StorePlatformType.APP_STORE => "\uf8ff ", //
				StorePlatformType.GOOGLE_PLAY => "\u25B7 ", // ▷
				_ => string.Empty
			};

			return $"{prefix}{platform}";
		}
	}
}
