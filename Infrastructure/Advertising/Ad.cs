namespace Advertising
{
	public class AdvertisingEventsObserver
	{

		public AdvertisingEventsObserver()
		{
			AdManager.Events.RewardedDisplayed += OnRewardedDisplayed;
			AdManager.Events.RewardedClosed += OnRewardedClosed;
			AdManager.Events.RewardedDisplayFailed += OnRewardedDisplayFailed;

			AdManager.Events.InterstitialDisplayed += OnInterstitialDisplayed;
			AdManager.Events.InterstitialClosed += OnInterstitialClosed;
			AdManager.Events.InterstitialDisplayFailed += OnInterstitialDisplayFailed;
		}

		public void Dispose()
		{
			AdManager.Events.RewardedDisplayed -= OnRewardedDisplayed;
			AdManager.Events.RewardedClosed -= OnRewardedClosed;
			AdManager.Events.RewardedDisplayFailed -= OnRewardedDisplayFailed;

			AdManager.Events.InterstitialDisplayed -= OnInterstitialDisplayed;
			AdManager.Events.InterstitialClosed -= OnInterstitialClosed;
			AdManager.Events.InterstitialDisplayFailed -= OnInterstitialDisplayFailed;
		}

		private void OnRewardedDisplayed(AdPlacementEntry placement, object _) => OnAdStarted(placement); // ⏹
		private void OnRewardedClosed(AdPlacementEntry placement, bool full, object _) => OnAdFinished(placement, full); // ▶
		private void OnRewardedDisplayFailed(AdPlacementEntry placement, string _, object __) => OnAdFinished(placement, false); // ▶

		private void OnInterstitialDisplayed(AdPlacementEntry placement, object _) => OnAdStarted(placement); // ⏹
		private void OnInterstitialClosed(AdPlacementEntry placement, object _) => OnAdFinished(placement); // ▶
		private void OnInterstitialDisplayFailed(AdPlacementEntry placement, string _, object __) => OnAdFinished(placement); // ▶

		private void OnAdStarted(AdPlacementEntry placement) // ⏹
		{
			AdsDebug.Log($"[{placement.Type}] [ {placement.Id} ] display has begun");
		}

		private void OnAdFinished(AdPlacementEntry placement, bool full = true) // ▶
		{
			var postfix = string.Empty;
			if (!full)
				postfix = " (not full)";
			AdsDebug.Log($"[{placement.Type}] [ {placement.Id} ] display has ended{postfix}");
		}
	}
}
