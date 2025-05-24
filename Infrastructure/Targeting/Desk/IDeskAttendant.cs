using Targeting;
using Sapientia;

namespace Targeting
{
	public interface IDeskAttendant
	{
		public string Identifier { get; }

		public string GetVersion();
		public ref readonly PlatformEntry GetPlatform();
		public ref readonly StorePlatformEntry GetStorePlatform();
		public string GetReviewLink(StorePlatformEntry store);

		public IReactiveProperty<string> UserId { get; }
		internal void SetUserId(string userId);
	}
}
