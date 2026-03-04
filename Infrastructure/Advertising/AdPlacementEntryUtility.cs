using Content;

namespace Advertising
{
	public static class AdPlacementEntryUtility
	{
		public static AdPlacementEntry GetEntry(AdPlacementType type, string placement)
			=> type switch
			{
				AdPlacementType.Rewarded => ContentManager.Get<RewardedAdPlacementEntry>(placement),
				AdPlacementType.Interstitial => ContentManager.Get<InterstitialAdPlacementEntry>(placement),
				_ => null
			};

		public static bool TryGetEntry(AdPlacementType type, string placement, out AdPlacementEntry entry)
		{
			entry = null;
			switch (type)
			{
				case AdPlacementType.Rewarded:
					if (ContentManager.TryGetEntry<RewardedAdPlacementEntry>(placement, out var rEntry))
					{
						entry = rEntry;
						return true;
					}

					break;
				case AdPlacementType.Interstitial:
					if (ContentManager.TryGetEntry<InterstitialAdPlacementEntry>(placement, out var iEntry))
					{
						entry = iEntry;
						return true;
					}

					break;
			}

			return false;
		}
	}
}
