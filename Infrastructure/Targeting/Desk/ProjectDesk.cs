using System.Runtime.CompilerServices;
using Sapientia;

namespace Targeting
{
	/// <summary>
	/// Стойка с информацией о проекте и его дистрибуции (платформа, страна)
	/// </summary>
	public class ProjectDesk : StaticProvider<IProjectDeskAttendant>
	{
		private static IProjectDeskAttendant attendant
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}

		public static bool IsInitialized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance != null;
		}

		public static string Version => attendant.GetVersion();
		public static string Identifier => attendant.Identifier;

		public static ref readonly PlatformEntry Platform => ref attendant.GetPlatform();
		public static ref readonly StorePlatformEntry StorePlatform => ref attendant.GetStorePlatform();
		public static IReactiveProperty<string> UserId => attendant.UserId;

		public static string GetReviewLink() => GetReviewLink(StorePlatform);

		public static string GetReviewLink(StorePlatformEntry store) => attendant.GetReviewLink(store);

		public static void SetUserId(string userId) => attendant.SetUserId(userId);
	}
}
