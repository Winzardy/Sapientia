using Sapientia;
using Sapientia.Collections;
using Submodules.Sapientia.Data;

namespace Submodules.Sapientia.Memory
{
	public static class MemoryManagerController
	{
#if UNITY_5_3_OR_NEWER
		private struct InnerContext {}
		private static readonly Unity.Burst.SharedStatic<MemoryManager> _inner = Unity.Burst.SharedStatic<MemoryManager>.GetOrCreate<MemoryManager, InnerContext>();
		private struct TempContext {}
		private static readonly Unity.Burst.SharedStatic<MemoryManager> _temp = Unity.Burst.SharedStatic<MemoryManager>.GetOrCreate<MemoryManager, TempContext>();
		private struct TempParallelContext {}
		private static readonly Unity.Burst.SharedStatic<MemoryManager> _tempParallel = Unity.Burst.SharedStatic<MemoryManager>.GetOrCreate<MemoryManager, TempParallelContext>();
		private struct ParallelContext {}
		private static readonly Unity.Burst.SharedStatic<MemoryManager> _parallel = Unity.Burst.SharedStatic<MemoryManager>.GetOrCreate<MemoryManager, ParallelContext>();
		private struct DefaultContext {}
		private static readonly Unity.Burst.SharedStatic<MemoryManager> _default = Unity.Burst.SharedStatic<MemoryManager>.GetOrCreate<MemoryManager, DefaultContext>();
		private struct ManagersContext {}
		private static readonly Unity.Burst.SharedStatic<UnsafeIndexAllocSparseSet<MemoryManager>> _managers = Unity.Burst.SharedStatic<UnsafeIndexAllocSparseSet<MemoryManager>>.GetOrCreate<UnsafeIndexAllocSparseSet<MemoryManager>, ManagersContext>();

		private static ref MemoryManager Inner => ref _inner.Data;
		private static ref MemoryManager Temp => ref _temp.Data;
		private static ref MemoryManager NoTrackTemp => ref _tempParallel.Data;
		private static ref MemoryManager NoTrack => ref _parallel.Data;
		private static ref MemoryManager Default => ref _default.Data;
		private static ref UnsafeIndexAllocSparseSet<MemoryManager> Managers => ref _managers.Data;
#else
		private static MemoryManager Inner;
		private static MemoryManager Temp;
		private static MemoryManager NoTrackTemp;
		private static MemoryManager NoTrack;
		private static MemoryManager Default;
		private static UnsafeIndexAllocSparseSet<MemoryManager> Managers;
#endif
#if DEEP_MEMORY_TRACKING
		private const TrackingType InnerTrackingType = TrackingType.NoTracking;
		private const TrackingType TempTrackingType = TrackingType.DeepTracking;
		private const TrackingType NoTrackTempTrackingType = TrackingType.NoTracking;
		private const TrackingType NoTrackTrackingType = TrackingType.NoTracking;
		private const TrackingType DefaultTrackingType = TrackingType.DeepTracking;
#else
		private const TrackingType InnerTrackingType = TrackingType.NoTracking;
		private const TrackingType TempTrackingType = TrackingType.CountTracking;
		private const TrackingType NoTrackTempTrackingType = TrackingType.CountTracking;
		private const TrackingType NoTrackTrackingType = TrackingType.NoTracking;
		private const TrackingType DefaultTrackingType = TrackingType.DeepTracking;
#endif

#if UNITY_5_3_OR_NEWER
		[UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
		public static void Initialize()
		{
			Inner = new MemoryManager(MemoryManager.InnerMemoryId);
			Inner.SetTracker(InnerTrackingType);

#if UNITY_5_3_OR_NEWER
			Temp = new MemoryManager(MemoryManager.TempMemoryId, Unity.Collections.Allocator.Temp);
			NoTrackTemp = new MemoryManager(MemoryManager.NoTrackTempMemoryId, Unity.Collections.Allocator.Temp);
#else
			Temp = new MemoryManager(MemoryManager.TempMemoryId);
			NoTrackTemp = new MemoryManager(MemoryManager.NoTrackTempMemoryId);
#endif
			Temp.SetTracker(TempTrackingType);
			NoTrackTemp.SetTracker(NoTrackTempTrackingType);

			NoTrack = new MemoryManager(MemoryManager.NoTrackMemoryId);
			NoTrack.SetTracker(NoTrackTrackingType);

			Default = new MemoryManager(MemoryManager.DefaultMemoryId);
			Default.SetTracker(DefaultTrackingType);

			Managers = new UnsafeIndexAllocSparseSet<MemoryManager>(8, 8);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
		}

		public static Id<MemoryManager> CreateMemoryManager(TrackingType trackingType = TrackingType.DeepTracking)
		{
			var result = (Id<MemoryManager>)Managers.AllocateId();
			ref var manager = ref Managers.Get(result);
			manager = new MemoryManager(result);
			manager.SetTracker(trackingType);

			return result;
		}

		public static ref MemoryManager GetMemoryManager(Id<MemoryManager> id = default)
		{
			switch (id.ToMemoryType())
			{
				case MemoryType.Invalid:
					break;
				case MemoryType.Default:
					return ref Default;
				case MemoryType.Temp:
					return ref Temp;
				case MemoryType.NoTrack:
					return ref NoTrack;
				case MemoryType.NoTrackTemp:
					return ref NoTrackTemp;
				case MemoryType.Inner:
					return ref Inner;
				default:
					break;
			}

			return ref Managers.Get(id);
		}

		public static void DisposeMemoryManager(Id<MemoryManager> id)
		{
			E.ASSERT(id.ToMemoryType() == MemoryType.Invalid);

			ref var manager = ref Managers.Get(id);
			manager.Dispose();

			Managers.ReleaseId(id);
		}

#if UNITY_EDITOR
		private static void PlayModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state != UnityEditor.PlayModeStateChange.EnteredEditMode)
				return;

			UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			DisposeAll();
		}
#endif

		public static void DisposeAll()
		{
			foreach (ref var manager in Managers.GetValuesSpan())
			{
				manager.Dispose();
			}

			Managers.Dispose();
			Default.Dispose();
			Temp.Dispose();
			NoTrackTemp.Dispose();
			NoTrack.Dispose();
			Inner.Dispose();
		}
	}
}
