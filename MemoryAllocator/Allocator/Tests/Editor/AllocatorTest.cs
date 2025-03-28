using NUnit.Framework;
using Sapientia.Collections;

namespace Sapientia.MemoryAllocator.Tests
{
	public unsafe class AllocatorTest
	{
		[Test]
		public void Test()
		{
			var allocator = AllocatorManager.CreateAllocator();

			var memPtrs = new SimpleList<MemPtr>();
			var count = 1024 * 4;
			for (var i = 0; i < count; i++)
			{
				memPtrs.Add(allocator.Value().MemAlloc<int>());
			}

			var odd = true;
			foreach (var memPtr in memPtrs)
			{
				if (odd)
				{
					odd = false;
					memPtr.Dispose();
				}
				else
					odd = true;
			}

			allocator.Value().Dispose();
		}
	}
}
