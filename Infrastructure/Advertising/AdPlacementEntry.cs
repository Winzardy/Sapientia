#if DebugLog
#define ENABLE_AD_EMPTY_CHECK
#endif
using System;
using System.Collections.Generic;
using Targeting;
using Content;
using Sapientia;
#if ENABLE_AD_EMPTY_CHECK
using Sapientia.Extensions;
#endif

namespace Advertising
{
	[Serializable]
	[Constants]
	public class InterstitialAdPlacementEntry : AdPlacementEntry
	{
		public override AdPlacementType Type => AdPlacementType.Interstitial;
	}

	[Serializable]
	[Constants]
	public class RewardedAdPlacementEntry : AdPlacementEntry
	{
		public override AdPlacementType Type => AdPlacementType.Rewarded;
	}

	public abstract class AdPlacementEntry : IExternallyIdentifiable
	{
		/// <summary>
		/// Этот идентификатор служит определением рекламы внутри проекта, Id который в итоге используется в <see cref="Advertising"/> может отличаться!
		/// </summary>
		internal string id;

		public abstract AdPlacementType Type { get; }

		/// <summary>
		/// Обычно имя совпадает с id, можно установить свое, но есть также Platform -> Name, что приоритетнее
		/// </summary>
		public bool useCustomName;

		public string customName;

		public Dictionary<PlatformEntry, string> platformToName;

		/// <summary>
		/// Передавать ли 'placement' в интеграцию
		/// </summary>
		public bool integrationTrack;

		public UsageLimitEntry usageLimit;

		// TODO: добавить Dictionary<Condition, UsageLimitEntry>
		// нужно для случаев если хотим переопределить лимит по условиям


		public string GetName(in PlatformEntry platform)
		{
#if ENABLE_AD_EMPTY_CHECK
			if (id.IsNullOrEmpty())
				throw new Exception("AdPlacementEntry name is empty!");
#endif
			if (platform && platformToName.TryGetValue(platform, out var name))
				return name;

			return useCustomName ? customName : id;
		}

		public static implicit operator AdPlacementType(AdPlacementEntry entry) => entry.Type;
		public static implicit operator bool(AdPlacementEntry entry) => entry != null;

		/// <summary>
		/// <inheritdoc cref="id"/>
		/// </summary>
		public string Id => id;

		void IExternallyIdentifiable.SetId(string id) => this.id = id;
	}

	public static class AdPlacementEntryExt
	{
		public static bool CanShow<T>(this T entry, out AdShowError? error) where T : AdPlacementEntry
			=> AdManager.CanShow(entry, out error);

		public static bool Show<T>(this T entry, bool autoLoad = true) where T : AdPlacementEntry
			=> AdManager.Show(entry, autoLoad);

		public static bool Load<T>(this T entry) where T : AdPlacementEntry
			=> AdManager.Load(entry);
	}
}
