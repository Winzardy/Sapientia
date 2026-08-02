using Sapientia.Collections;
using Sapientia.Extensions;
using System.Collections.Generic;

namespace Sapientia.Localization
{
	public interface IRemoteLocalizationsContainer
	{
		string GetString(string key, string languageIso, bool fallbackToDefaultLang = true);
		bool HasKey(string key, string languageIso);

		void AddStrings(RemoteLocalizationStrings strings);
	}

	public class RemoteLocalizationsContainer : IRemoteLocalizationsContainer
	{
		private const string DEFAULT_LANG_ISO = "en";

		// key -> lang/localized string.
		private Dictionary<string, Dictionary<string, string>> _map = new Dictionary<string, Dictionary<string, string>>();

		public string GetString(string key, string languageIso, bool fallbackToDefaultLang = true)
		{
			if (!_map.TryGetValue(key, out var langStringDict))
			{
				throw new KeyNotFoundException($"No remotely loaded localization entry for key [ {key} ].");
			}

			if (langStringDict.TryGetValue(languageIso, out var localizedString))
			{
				return localizedString;
			}

			if (fallbackToDefaultLang && languageIso != DEFAULT_LANG_ISO
				&& langStringDict.TryGetValue(DEFAULT_LANG_ISO, out localizedString))
			{
				return localizedString;
			}

			throw new KeyNotFoundException($"No string for key [ {key} ] in [ {languageIso} ] or default language.");
		}

		public bool HasKey(string key, string languageIso)
		{
			return _map.TryGetValue(key, out var dict) && dict.ContainsKey(languageIso);
		}

		public void AddStrings(RemoteLocalizationStrings strings)
		{
			if (strings.languagePairs.IsNullOrEmpty())
				return;

			if (!_map.TryGetValue(strings.key, out var langStringDict))
			{
				langStringDict = new Dictionary<string, string>();
				_map[strings.key] = langStringDict;
			}

			for (int i = 0; i < strings.languagePairs.Length; i++)
			{
				var pair = strings.languagePairs[i];
				langStringDict[pair.languageIso] = pair.localizedString;
			}
		}
	}
}
