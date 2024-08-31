#define LOGS_ENABLED

#if LOGS_ENABLED && UNITY_EDITOR

using Sapientia.MemoryAllocator.Data;

namespace Sapientia.MemoryAllocator
{
	public partial struct Allocator
	{
		public static bool startLog;
		public static System.Collections.Generic.Dictionary<MemPtr, string> logList = new ();

		[Unity.Burst.BurstDiscardAttribute]
		public static void LogAdd(in MemPtr memPtr, long size)
		{
			if (startLog)
			{
				var str = "ALLOC: " + memPtr + ", SIZE: " + size;
				logList.Add(memPtr, str + "\n" + UnityEngine.StackTraceUtility.ExtractStackTrace());
			}
		}

		[Unity.Burst.BurstDiscardAttribute]
		public static void LogRemove(in MemPtr memPtr)
		{
			logList.Remove(memPtr);
		}

		[UnityEditor.MenuItem("Debug/Allocator: Start Log")]
		public static void StartLog()
		{
			startLog = true;
		}

		[UnityEditor.MenuItem("Debug/Allocator: End Log")]
		public static void EndLog()
		{
			startLog = false;
			logList.Clear();
		}

		[UnityEditor.MenuItem("Debug/Allocator: Print Log")]
		public static void PrintLog()
		{
			foreach (var item in logList)
			{
				UnityEngine.Debug.Log(item.Key + "\n" + item.Value);
			}
		}
	}
}

#endif
