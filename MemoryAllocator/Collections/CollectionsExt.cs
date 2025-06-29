namespace Sapientia.MemoryAllocator
{
	public static unsafe class CollectionsExt
	{
		public static void FillIndexes(this ref List<int> list, World world)
		{
			var values = list.GetValuePtr(world);
			for (var i = 0; i < list.Count; i++)
			{
				values[i] = i;
			}
		}

		public static void Shuffle<T>(this ref List<T> list, World world) where T: unmanaged
		{
			var values = list.GetValuePtr(world);
			var count = list.Count;
			for (var i = 0; i < count; i++)
			{
#if UNITY_5_3_OR_NEWER
				var indexToSwap = UnityEngine.Random.Range(i, count);
#else
				var indexToSwap = System.Random.Shared.Next(i, count);
#endif
				(values[i], values[indexToSwap]) = (values[indexToSwap], values[i]);
			}
		}

	}
}
