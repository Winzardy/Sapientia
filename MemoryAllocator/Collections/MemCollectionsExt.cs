namespace Sapientia.MemoryAllocator
{
	public static class MemCollectionsExt
	{
		public static void FillIndexes(this ref MemList<int> list, WorldState worldState)
		{
			var values = list.GetValuePtr(worldState);
			for (var i = 0; i < list.Count; i++)
			{
				values[i] = i;
			}
		}

		public static void Shuffle<T>(this ref MemList<T> list, WorldState worldState) where T: unmanaged
		{
			var values = list.GetValuePtr(worldState);
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
