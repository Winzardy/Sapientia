using System.IO;
using System.Threading.Tasks;

namespace Sapientia.Extensions;

public static class StreamExtensions
{
	public static async Task<string> ReadToEndAsync(this Stream stream)
	{
		using var reader = new StreamReader(stream);
		var result = await reader.ReadToEndAsync();
		return result;
	}
}