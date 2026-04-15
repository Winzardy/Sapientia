using System;
using Sapientia;

namespace ProjectInformation
{
	/// <summary>
	/// Runtime информация о проекте и дистрибуции проекта (платформа, страна)
	/// </summary>
	public class ProjectInfo : StaticWrapper<IProjectInfoAttendant>
	{
		private static IProjectInfoAttendant attendant { get => _instance; }
		public static bool IsInitialized { get => _instance != null; }

		public static string Version { get => attendant.GetVersion(); }
		public static BuildInfo Build { get => attendant.GetBuildInfo(); }
		public static string Identifier { get => attendant.Identifier; }

		public static ref readonly PlatformEntry Platform { get => ref attendant.GetPlatform(); }
		public static ref readonly DistributionEntry Distribution { get => ref attendant.GetDistribution(); }

		public static string GetReviewLink() => GetReviewLink(Distribution);
		public static string GetReviewLink(DistributionEntry store) => attendant.GetStoreUrl(store);

		public static string GetStoreUrl() => GetStoreUrl(Distribution);
		public static string GetStoreUrl(DistributionEntry store) => attendant.GetStoreUrl(store);

		public static string UserId { get => attendant.UserId; }
		public static void SetUserId(string userId) => attendant.SetUserId(userId);
		public static event Action UserIdChanged
		{
			add => attendant.UserIdChanged += value;
			remove => attendant.UserIdChanged -= value;
		}
	}
}
