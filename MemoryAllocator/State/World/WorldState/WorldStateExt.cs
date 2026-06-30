using System.Runtime.CompilerServices;
using Sapientia.Collections.FixedString;
using Sapientia.MemoryAllocator.State;

namespace Sapientia.MemoryAllocator
{
	public static class WorldStateExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity CreateEntity(this WorldState worldState, in FixedString64Bytes name = default)
		{
			return worldState.GetService<EntityStatePart>().CreateEntity(worldState, name);
		}
	}
}
