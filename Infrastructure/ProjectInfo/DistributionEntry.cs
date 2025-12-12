using System;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace ProjectInformation
{
	public class DistributionType
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
	public partial struct DistributionEntry
	{
#if CLIENT
		[UnityEngine.Serialization.FormerlySerializedAs("store")]
#endif
		public string name;

		public DistributionEntry(string name) => this.name = name;

		public static implicit operator string(DistributionEntry platform) => platform.name;
		public static implicit operator DistributionEntry(string name) => new(name);

		public static implicit operator bool(DistributionEntry platform) =>
			!platform.name.IsNullOrEmpty() || platform.name == DistributionType.UNDEFINED;

		public override int GetHashCode() => name?.GetHashCode() ?? 0;

		public override string ToString() => name ?? DistributionType.UNDEFINED;
	}

	public static class DistributionUtility
	{
		public static IEnumerable<DistributionEntry> GetAllPlatforms()
		{
			yield return DistributionType.GOOGLE_PLAY;
			yield return DistributionType.APP_STORE;
			yield return DistributionType.AMAZON;
		}

		public static string ToLabel(this in DistributionEntry entry) => GetLabel(entry.name);

		public static string GetLabel(string type)
		{
			var prefix = type switch
			{
				DistributionType.APP_STORE => "\uf8ff ", //
				DistributionType.GOOGLE_PLAY => "\u25B7 ", // ▷
				_ => string.Empty
			};

			return $"{prefix}{type}";
		}
	}
}
