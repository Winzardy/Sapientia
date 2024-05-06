#if !UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	public static class СsvExt
	{
#region From
		public static SimpleList<T> FromCsv<T>(this TextReader reader, bool hasHeader = true)
		{
			var values = new SimpleList<T>();
			using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture){ HasHeaderRecord = hasHeader });
			// CsvReader actually reads values only when you iterate the GetRecords IEnumerable
			// You can not use this IEnumerable outside CsvReader using block!
			foreach (var record in csvReader.GetRecords<T>())
				values.Add(record);
			return values;
		}

		public static SimpleList<T> FromCsv<T>(this string csv, bool hasHeader = true)
		{
			using var reader = new StringReader(csv);
			return FromCsv<T>(reader, hasHeader);
		}

		public static SimpleList<T> FromCsvFile<T>(this string filePath, bool hasHeader = true)
		{
			using var reader = new StreamReader(filePath);
			return FromCsv<T>(reader, hasHeader);
		}

		public static bool TryFromCsvFile<T>(this string filePath, out SimpleList<T> data, bool hasHeader = true)
		{
			using var reader = new StreamReader(filePath);
			data = FromCsv<T>(reader, hasHeader);
			return data.Count > 0;
		}
#endregion

#region To
		public static void ToCsv<T>(this IEnumerable<T> values, TextWriter writer, bool hasHeader = true)
		{
			using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = hasHeader });
			csvWriter.WriteRecords(values);
		}

		public static string ToCsv<T>(this IEnumerable<T> values, bool hasHeader = true)
		{
			using var writer = new StringWriter();
			values.ToCsv(writer, hasHeader);
			return writer.ToString();
		}

		public static void ToCsvFile<T>(this IEnumerable<T> values, string filePath, bool hasHeader = true)
		{
			using var writer = new StreamWriter(filePath);
			values.ToCsv(writer, hasHeader);
		}
#endregion
	}
}
#endif