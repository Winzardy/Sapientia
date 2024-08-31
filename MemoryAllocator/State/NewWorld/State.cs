using Sapientia.MemoryAllocator.Core;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public class State
	{
		public AllocatorId allocatorId;

		public static State Create(int initialSize = -1, int maxSize = -1)
		{
			var allocatorId = AllocatorManager.CreateAllocator(initialSize, maxSize);
			var world = World.Create(allocatorId);

			return new State { allocatorId = allocatorId };
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			ref var allocator = ref allocatorId.GetAllocator();

			allocator.Serialize(ref stream);
		}

		public State Deserialize(ref StreamBufferReader stream)
		{
			var state = new State();

			state.allocatorId = AllocatorManager.DeserializeAllocator(ref stream);

			return state;
		}
	}
}
