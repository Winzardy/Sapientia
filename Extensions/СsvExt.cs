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
			using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = hasHeader,
				HeaderValidated = null,
				MissingFieldFound = null,
			});
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

		public static SimpleList<T> FromCsvObjects<T>(this IEnumerable<IEnumerable<object>> values)
		{
			var csv = values.ObjectsToCsv();
			return csv.FromCsv<T>();
		}

		private static IList<IList<T>> FromCsvToValues<T>(this TextReader reader) where T: class
		{
			var csvValues = new SimpleList<IList<T>>();
			using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false,
				MissingFieldFound = null,
			});

			while (csvReader.Read())
			{
				var row = new SimpleList<T>();
				while (csvReader.TryGetField(row.Count, out string? value))
				{
					row.Add((value as T)!);
				}
				csvValues.Add(row);
			}

			return csvValues;
		}

		public static IList<IList<object>> FromCsvToObjectValues(this string csv)
		{
			return csv.FromCsvToValues<object>();
		}

		public static IList<IList<string>> FromCsvToStringValues(this string csv)
		{
			return csv.FromCsvToValues<string>();
		}

		private static IList<IList<T>> FromCsvToValues<T>(this string csv) where T: class
		{
			using var reader = new StringReader(csv);
			return reader.FromCsvToValues<T>();
		}

		public static IList<IList<object>> FromCsvFileToObjectValues(this string filePath)
		{
			return filePath.FromCsvFileToValues<object>();
		}

		public static IList<IList<string>> FromCsvFileToStringValues(this string filePath)
		{
			return filePath.FromCsvFileToValues<string>();
		}

		private static IList<IList<T>> FromCsvFileToValues<T>(this string filePath) where T: class
		{
			using var reader = new StreamReader(filePath);
			return reader.FromCsvToValues<T>();
		}

#endregion

#region To
		public static void WriteToCsv<T>(this IEnumerable<T> values, TextWriter writer, bool hasHeader = true)
		{
			using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = hasHeader,
			});
			csvWriter.WriteRecords(values);
		}

		public static void ToCsvFile<T>(this IEnumerable<T> values, string filePath, bool hasHeader = true)
		{
			using var writer = new StreamWriter(filePath);
			values.WriteToCsv(writer, hasHeader);
		}

		public static string ToCsv<T>(this IEnumerable<T> values, bool hasHeader = true)
		{
			using var writer = new StringWriter();
			values.WriteToCsv(writer, hasHeader);
			return writer.ToString();
		}

		public static void ObjectsToCsvFile(this IEnumerable<IEnumerable<object>> values, string filePath)
		{
			using var writer = new StreamWriter(filePath);
			values.WriteObjectsToCsv(writer);
		}

		public static string ObjectsToCsv(this IEnumerable<IEnumerable<object>> values)
		{
			using var writer = new StringWriter();
			values.WriteObjectsToCsv(writer);
			return writer.ToString();
		}

		public static void WriteObjectsToCsv(this IEnumerable<IEnumerable<object>> values, TextWriter writer)
		{
			foreach (var row in values)
			{
				var firstColumn = true;
				foreach (var field in row)
				{
					if (!firstColumn)
						writer.Write(',');

					var formattedField = field.ToString()!.Replace("\"", "\"\""); // Escape double quotes
					formattedField = $"\"{formattedField}\""; // Wrap the field in double quotes
					writer.Write(formattedField);

					firstColumn = false;
				}
				writer.WriteLine();
			}
			writer.Flush();
		}
#endregion
	}
}
#endif