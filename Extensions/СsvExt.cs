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
		public static SimpleList<T> FromCsv<T>(this TextReader reader)
		{
			var values = new SimpleList<T>();
			using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
			// CsvReader actually reads values only when you iterate the GetRecords IEnumerable
			// You can not use this IEnumerable outside CsvReader using block!
			foreach (var record in csvReader.GetRecords<T>())
				values.Add(record);
			return values;
		}

		public static SimpleList<T> FromCsv<T>(this string csv)
		{
			using var reader = new StringReader(csv);
			return FromCsv<T>(reader);
		}

		public static SimpleList<T> FromCsvFile<T>(this string filePath)
		{
			using var reader = new StreamReader(filePath);
			return FromCsv<T>(reader);
		}
#endregion

#region To
		public static void ToCsv<T>(this IEnumerable<T> values, TextWriter writer)
		{
			using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
			csvWriter.WriteRecords(values);
		}

		public static string ToCsv<T>(this IEnumerable<T> values)
		{
			using var writer = new StringWriter();
			using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
			csvWriter.WriteRecords(values);

			return writer.ToString();
		}

		public static void ToCsvFile<T>(this IEnumerable<T> values, string filePath)
		{
			using var writer = new StreamWriter(filePath);
			using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
			csvWriter.WriteRecords(values);
		}
#endregion
	}
}
#endif