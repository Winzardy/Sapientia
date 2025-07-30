using System;

namespace Advertising
{
	public struct AdPlacementKey
	{
		public string placement;
		public AdPlacementType type;

		public AdPlacementKey(AdPlacementType type, string placement)
		{
			this.type = type;
			this.placement = placement;
		}

		public static implicit operator AdPlacementKey((AdPlacementType type, string id) tuple)
			=> new(tuple.type, tuple.id);

		public static implicit operator string(AdPlacementKey key)
			=> key.ToString();

		public static implicit operator AdPlacementKey(AdPlacementEntry entry)
			=> new(entry.Type, entry.id);

		public override string ToString() => $"{type}:{placement}";
		public override int GetHashCode() => HashCode.Combine(type, placement);
	}

	public static class AdPlacementKeyExtensions
	{
		public static AdPlacementKey ToKey(this AdPlacementType type, string placement) => new(type, placement);

		public static AdPlacementEntry GetEntry(this AdPlacementKey key) => AdManager.GetEntry(key.type, key.placement);
		public static bool TryGetEntry(this AdPlacementKey key, out AdPlacementEntry entry)
			=> AdManager.TryGetEntry(key.type, key.placement, out entry);
	}
}
