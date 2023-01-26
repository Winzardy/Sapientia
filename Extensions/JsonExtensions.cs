using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	public static class JsonExtensions
	{
		private static readonly JsonSerializerSettings JSON_SETTINGS = new ()
		{
			TypeNameHandling = TypeNameHandling.Auto,
		};

		public static string ToJson<T>(this T from)
		{
			return JsonConvert.SerializeObject(from, JSON_SETTINGS);
		}

		public static void AppendToJsonFile<T>(this T from, string filePath)
		{
			using var streamWriter = File.AppendText(filePath);

			var json = from.ToJson();
			streamWriter.WriteLine(json);
		}

		public static void AppendToJsonFile<T>(this T from, StreamWriter streamWriter)
		{
			var json = from.ToJson();
			streamWriter.WriteLine(json);
		}

		public static bool TryFromJson<T>(this string json, out T value)
		{
			try
			{
				value = JsonConvert.DeserializeObject<T>(json, JSON_SETTINGS)!;
				return true;
			}
			catch (Exception e)
			{
				value = default!;
				return false;
			}
		}

		public static T FromJson<T>(this string json)
		{
			return JsonConvert.DeserializeObject<T>(json, JSON_SETTINGS)!;
		}

		public static T FromJsonFile<T>(this string path)
		{
			var json = File.ReadAllText(path);
			return json.FromJson<T>();
		}

		public static async Task<T> FromJsonFileAsync<T>(this string filePath)
		{
			var json = await File.ReadAllTextAsync(filePath);
			return json.FromJson<T>();
		}

		public static SimpleList<T> ListFromJson<T>(this string fileString)
		{
			if (string.IsNullOrEmpty(fileString))
				return default!;

			using var stringReader = new StringReader(fileString);
			var changeItemEvents = ListFromJson<T>(stringReader);

			return changeItemEvents;
		}

		public static SimpleList<T> ListFromJson<T>(this TextReader textReader)
		{
			var changeItemEvents = new SimpleList<T>();

			while (textReader.Peek() > 0)
			{
				var json = textReader.ReadLine()!;
				var changeItemEvent = json.FromJson<T>();
				changeItemEvents.Add(changeItemEvent);
			}

			return changeItemEvents;
		}
	}
}