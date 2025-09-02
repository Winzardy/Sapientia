using System;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Targeting
{
	/// <summary>
	/// Поддерживаемые платформы (supported)
	/// </summary>
	public class PlatformType
	{
		public const string UNDEFINED = "Undefined";

		public const string IOS = "iOS";
		public const string ANDROID = "Android";
	}

	/// <summary>
	/// Платформа приложения/Операционная система (iOS, Android, Windows...)
	/// </summary>
	[Serializable]
	public partial struct PlatformEntry
	{
		public string platform;

		public PlatformEntry(string platform) => this.platform = platform;

		public static implicit operator string(PlatformEntry platform) => platform.platform;
		public static implicit operator PlatformEntry(string name) => new(name);

		public static implicit operator bool(PlatformEntry platform) =>
			!platform.platform.IsNullOrEmpty() || platform.platform == PlatformType.UNDEFINED;

		public override int GetHashCode() => platform?.GetHashCode() ?? 0;

		public override string ToString() => platform ?? PlatformType.UNDEFINED;
	}

	public static class PlatformUtility
	{
		public static IEnumerable<PlatformEntry> GetAllPlatforms()
		{
			yield return PlatformType.ANDROID;
			yield return PlatformType.IOS;
		}

		public static string ToLabel(this in PlatformEntry entry) => GetLabel(entry);

		public static string GetLabel(string platform)
		{
			var prefix = platform switch
			{
				PlatformType.IOS => "\uf8ff ", // 
				PlatformType.ANDROID => "\ud83e\udd16", // 🤖
				_ => string.Empty
			};

			return $"{prefix}{platform}";
		}
	}
}
