using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemZone
	{
		public int size; // total bytes malloced, including header
		public MemBlock blocklist; // start / end cap for linked list
		public MemBlockOffset rover;
	}
}
