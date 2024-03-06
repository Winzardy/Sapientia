using System;
using System.IO;
using System.Threading.Tasks;
using Sapientia.Collections;

namespace Sapientia.Extensions
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#0c69c3bb23a34ef49e97b4efd8509e26
	/// </summary>
	public static class StreamExt
	{
		public static async Task<string> ReadToEndAsync(this Stream stream)
		{
			using var reader = new StreamReader(stream);
			var result = await reader.ReadToEndAsync();
			return result;
		}

		public static async Task<SimpleList<string>?> ReadLinesAsync(this Stream stream)
		{
			using var reader = new StreamReader(stream);
			return await reader.ReadLinesAsync();
		}

		public static async Task<SimpleList<string>?> ReadLinesAsync(this StreamReader reader)
		{
			if (reader.Peek() == 0)
				return default;

			var result = new SimpleList<string>();

			try
			{
				do
				{
					var value = (await reader.ReadLineAsync())!;
					result.Add(value);
				} while (reader.Peek() > 0);

				return result;
			}
			catch (Exception e)
			{
				return default;
			}
		}
	}
}