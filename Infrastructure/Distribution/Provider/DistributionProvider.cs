using System.Runtime.CompilerServices;
using Sapientia;

namespace Distribution
{
	public class DistributionProvider : StaticProvider<IDistributionProvider>
	{
		private static IDistributionProvider provider
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}

		public static bool IsInitialized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance != null;
		}

		public static string Version => provider.GetVersion();
		public static string Identifier => provider.Identifier;

		public static ref readonly PlatformEntry Platform => ref provider.GetPlatform();
		public static ref readonly StorePlatformEntry StorePlatform => ref provider.GetStorePlatform();
		public static IReactiveProperty<string> UserId => provider.UserId;

		public static string GetReviewLink() => GetReviewLink(StorePlatform);

		public static string GetReviewLink(StorePlatformEntry store) => provider.GetReviewLink(store);

		public static void SetUserId(string userId) => provider.SetUserId(userId);
	}
}
