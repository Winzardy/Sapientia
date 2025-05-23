using System;
using System.Collections.Generic;
using System.Globalization;
using Sapientia.Extensions;

namespace Distribution
{
	[Serializable]
	public struct CountryEntry
	{
		public const string UNKNOWN = "Unknown";

		public string code;
		public string name;

		public CountryEntry(string code, string name)
		{
			this.code = code;
			this.name = name;
		}

		public static implicit operator string(CountryEntry platform) => platform.code;
		public static implicit operator CountryEntry(string code) => CountryEntryExt.Get(code);

		public static implicit operator bool(CountryEntry platform) =>
			!platform.code.IsNullOrEmpty();

		public override int GetHashCode() => code?.GetHashCode() ?? 0;

		public override string ToString() => code ?? UNKNOWN;
	}

	public static class CountryEntryExt
	{
		private static Dictionary<string, CountryEntryRef> _codeToEntry;

		public static IEnumerable<CountryEntryRef> GetAll()
		{
			if (_codeToEntry != null)
				return _codeToEntry.Values;

			Collect();

			return _codeToEntry!.Values;
		}

		public static ref readonly CountryEntry Get(string code)
		{
			if (_codeToEntry == null)
				Collect();

			return ref _codeToEntry![code].entry;
		}

		/// <summary>
		/// Сам шрифт обязательно должен поддерживать флаги, в Unity Editor не работает...(
		/// </summary>
		public static string GetEmojiByCode(string code)
		{
			if (code.IsNullOrEmpty() || code.Length != 2)
				return "🏳";

			code = code.ToUpperInvariant();
			var firstLetter = 0x1F1E6 + (code[0] - 'A');
			var secondLetter = 0x1F1E6 + (code[1] - 'A');
			var emoji = char.ConvertFromUtf32(firstLetter) + char.ConvertFromUtf32(secondLetter);
			return emoji;
		}

		private static void Collect()
		{
			_codeToEntry = new(32);
			foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
			{
				var region = new RegionInfo(culture.Name);
				_codeToEntry[region.TwoLetterISORegionName] = new CountryEntry(region.TwoLetterISORegionName, region.EnglishName);
			}
		}

		public static string ToLabel(this in CountryEntry entry) => GetLabel(entry.code);

		public static string GetLabel(string code)
		{
			var name = _codeToEntry[code].entry.name;
			return $"[{code}] {name}";
		}

		public class CountryEntryRef
		{
			public CountryEntry entry;
			public static implicit operator CountryEntryRef(CountryEntry entry) => new CountryEntryRef {entry = entry};
		}
	}
}
