using Sapientia;

namespace ProjectInformation
{
	/// <summary>
	/// Runtime информация о проекте и дистрибуции проекта (платформа, страна)
	/// </summary>
	public class ProjectInfo : StaticProvider<IProjectInfoAttendant>
	{
		private static IProjectInfoAttendant attendant => _instance;
		public static bool IsInitialized => _instance != null;

		public static string Version => attendant.GetVersion();
		public static string Identifier => attendant.Identifier;

		public static ref readonly PlatformEntry Platform => ref attendant.GetPlatform();
		public static ref readonly DistributionEntry Distribution => ref attendant.GetDistribution();
		public static IReactiveProperty<string> UserId => attendant.UserId;

		public static string GetReviewLink() => GetReviewLink(Distribution);
		public static string GetReviewLink(DistributionEntry store) => attendant.GetStoreUrl(store);

		public static string GetStoreUrl() => GetStoreUrl(Distribution);
		public static string GetStoreUrl(DistributionEntry store) => attendant.GetStoreUrl(store);

		public static void SetUserId(string userId) => attendant.SetUserId(userId);
	}
}
