using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	public enum SerializationType
	{
		Cut,
		Auto,
		AutoIndented,
		Full,
		FullIndented,
	}

	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#e03680965d0c4b1e885f63b536043428
	/// </summary>
	public static class JsonExt
	{
		private static readonly JsonSerializerSettings JSON_SETTINGS_NONE_TYPED = new()
		{
			TypeNameHandling = TypeNameHandling.None,
		};

		private static readonly JsonSerializerSettings JSON_SETTINGS_AUTO_TYPED = new()
		{
			TypeNameHandling = TypeNameHandling.Auto,
		};

		private static readonly JsonSerializerSettings JSON_SETTINGS_AUTO_TYPED_INDENTED = new()
		{
			TypeNameHandling = TypeNameHandling.Auto,
			Formatting = Formatting.Indented,
		};

		private static readonly JsonSerializerSettings JSON_SETTINGS_FULL_TYPED = new()
		{
			TypeNameHandling = TypeNameHandling.All,
		};

		private static readonly JsonSerializerSettings JSON_SETTINGS_FULL_TYPED_INDENTED = new()
		{
			TypeNameHandling = TypeNameHandling.All,
			Formatting = Formatting.Indented,
		};

		public static string ToJson<T>(this T from, SerializationType serializationType = SerializationType.Cut)
		{
			var settings = serializationType switch
			{
				SerializationType.Cut => JSON_SETTINGS_NONE_TYPED,
				SerializationType.Auto => JSON_SETTINGS_AUTO_TYPED,
				SerializationType.AutoIndented => JSON_SETTINGS_AUTO_TYPED_INDENTED,
				SerializationType.Full => JSON_SETTINGS_FULL_TYPED,
				SerializationType.FullIndented => JSON_SETTINGS_FULL_TYPED_INDENTED,
				_ => JSON_SETTINGS_NONE_TYPED
			};
			return JsonConvert.SerializeObject(from, typeof(T), settings);
		}

		public static void WriteToJsonFile<T>(this T from, string filePath, SerializationType serializationType = SerializationType.Cut, bool isCut = true)
		{
			File.WriteAllText(filePath, from.ToJson(serializationType));
		}

		public static void AppendToJsonFile<T>(this T from, string filePath, SerializationType serializationType = SerializationType.Cut, bool isCut = true)
		{
			using var streamWriter = File.AppendText(filePath);

			var json = from.ToJson(serializationType);
			streamWriter.WriteLine(json);
		}

		public static void AppendToJsonFile<T>(this T from, StreamWriter streamWriter, SerializationType serializationType = SerializationType.Cut)
		{
			var json = from.ToJson(serializationType);
			streamWriter.WriteLine(json);
		}

		public static async Task AppendToJsonFileAsync<T>(this T from, StreamWriter streamWriter, SerializationType serializationType = SerializationType.Cut)
		{
			var json = from.ToJson(serializationType);
			await streamWriter.WriteLineAsync(json);
		}

		public static bool TryFromJson<T>(this string json, out T value)
		{
			try
			{
				value = JsonConvert.DeserializeObject<T>(json, JSON_SETTINGS_AUTO_TYPED)!;
				return true;
			}
			catch (Exception e)
			{
				value = default!;
				return false;
			}
		}

		public static bool TryFromJsonFile<T>(this string path, out T value)
		{
			if (!File.Exists(path))
			{
				value = default!;
				return false;
			}
			var json = File.ReadAllText(path);
			value = json.FromJson<T>();
			return true;
		}

		// If you see error in Newtonsoft.Json.Serialization.JsonArrayContract.CreateWrapper check this - https://github.com/jilleJr/Newtonsoft.Json-for-Unity/issues/77
		public static T FromJson<T>(this string json)
		{
			return JsonConvert.DeserializeObject<T>(json, JSON_SETTINGS_AUTO_TYPED)!;
		}

		public static T? FromJsonOrDefault<T>(this string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(json, JSON_SETTINGS_AUTO_TYPED)!;
			}
			catch (Exception e)
			{
				return default(T);
			}
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

		public static SimpleList<T>? ListFromJson<T>(this string fileString)
		{
			if (string.IsNullOrEmpty(fileString))
				return default;

			using var stringReader = new StringReader(fileString);
			var changeItemEvents = ListFromJson<T>(stringReader);

			return changeItemEvents;
		}

		public static SimpleList<T>? ListFromJson<T>(this TextReader textReader)
		{
			if (textReader.Peek() == 0)
				return default;

			var result = new SimpleList<T>();
			do
			{
				var json = textReader.ReadLine()!;
				var changeItemEvent = json.FromJson<T>();
				result.Add(changeItemEvent);
			} while (textReader.Peek() > 0);

			return result;
		}

		public static async Task<SimpleList<T>?> ListFromJsonAsync<T>(this string fileString)
		{
			if (string.IsNullOrEmpty(fileString))
				return default;

			using var stringReader = new StringReader(fileString);
			var changeItemEvents = await ListFromJsonAsync<T>(stringReader);

			return changeItemEvents;
		}

		public static async Task<SimpleList<T>?> ListFromJsonAsync<T>(this Stream stream)
		{
			using var streamReader = new StreamReader(stream);
			return await ListFromJsonAsync<T>(streamReader);
		}

		public static async Task<SimpleList<T>?> ListFromJsonAsync<T>(this TextReader textReader)
		{
			if (textReader.Peek() == 0)
				return default;

			var result = new SimpleList<T>();

			try
			{
				do
				{
					var json = (await textReader.ReadLineAsync())!;
					var value = json.FromJson<T>();
					result.Add(value);
				} while (textReader.Peek() > 0);

				return result;
			}
			catch (Exception e)
			{
				return default;
			}
		}

		public static SimpleList<T>? ListFromJsonLines<T>(this SimpleList<string> lines)
		{
			var result = new SimpleList<T>();

			try
			{
				for (var i = 0; i < lines.Count; i++)
				{
					var value = lines[i].FromJson<T>();
					result.Add(value);
				}

				return result;
			}
			catch (Exception e)
			{
				return default;
			}
		}
	}
}