using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

		public static async IAsyncEnumerable<string> ReadLinesAsync(this Stream stream)
		{
			using var reader = new StreamReader(stream);
			await foreach (var line in reader.ReadLinesAsync().ConfigureAwait(false))
				yield return line;
		}

		public static async IAsyncEnumerable<string> ReadLinesAsync(this StreamReader reader)
		{
			while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
			{
				yield return line;
			}
		}
	}
}
