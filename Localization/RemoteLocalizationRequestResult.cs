using System;

namespace Sapientia.Localization
{
	/// <summary>
	/// Can either have LocKey (embedded into client), or a pack of strings.
	/// Never both.
	/// </summary>
	[Serializable]
	public struct RemoteLocalizationRequestResult
	{
		/// <summary>
		/// Key for client-embedded localization.
		/// </summary>
		public string embeddedLocKey;

		/// <summary>
		/// Localized strings delivered inline by the server, one entry per language.
		/// Ignored if <see cref="embeddedLocKey"/> is provided.
		/// </summary>
		public RemoteLocalizationStrings remoteStrings;
	}

	/// <summary>
	/// A localization table row.
	/// Key + values per each language.
	/// </summary>
	[Serializable]
	public struct RemoteLocalizationStrings
	{
		public string key;
		public LangStringPair[] languagePairs;

		[Serializable]
		public struct LangStringPair
		{
			public string languageIso;
			public string localizedString;
		}
	}
}
