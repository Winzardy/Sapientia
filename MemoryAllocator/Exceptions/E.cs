namespace Sapientia.MemoryAllocator
{
	public partial class E
	{
		public const string ENABLE_EXCEPTIONS = "UNITY_EDITOR";

		public static string Format(string str)
		{
			return $"[Sapientia.MemoryAllocator] {str}";
		}
	}
}
