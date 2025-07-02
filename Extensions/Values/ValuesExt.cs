using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class ValuesExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Swap<T>(ref T a, ref T b)
		{
			(a, b) = (b, a);
		}
	}

	/// <summary>
	/// Класс расширений для работы с this, дабы избежать конфликтов с другими методами.
	/// </summary>
	public static class ValuesExtExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Swap<T>(this ref T a, ref T b) where T : struct
		{
			(a, b) = (b, a);
		}
	}
}
