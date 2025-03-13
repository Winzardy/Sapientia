namespace Sapientia.MemoryAllocator
{
	public unsafe partial struct Allocator
	{
		[System.Diagnostics.ConditionalAttribute(COND.ALLOCATOR_VALIDATION)]
		public void ValidateConsistency()
		{
			CheckConsistency();
		}

		public void CheckConsistency()
		{
			locker.SetBusy(true);

			for (var i = 0; i < zonesListCount; ++i)
			{
				var zone = zonesList[i];
				if (zone == null)
				{
					continue;
				}

				if (!MzCheckHeap(zone, out var blockIndex, out var index))
				{
#if UNITY_5_3_OR_NEWER
					UnityEngine.Debug.LogError($"zone {i}, block {blockIndex}, index {index}, thread {Unity.Jobs.LowLevel.Unsafe.JobsUtility.ThreadIndex}");
#else
					System.Diagnostics.Debug.WriteLine($"zone {i}, block {blockIndex}, index {index}, thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
#endif
				}
			}

			locker.SetFree(true);
		}
	}
}
