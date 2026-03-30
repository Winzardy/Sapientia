using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Memory;
using Sapientia.ServiceManagement;

namespace Sapientia.MemoryAllocator
{
	public static class WorldManager
	{
		private const ushort FIRST_VERSION = 1;

		private static WorldState _currentWorldState = default;
		private static World _currentWorld = null;

		private static ushort[] _versions = Array.Empty<ushort>();
		private static WorldState[] _worldsStates = Array.Empty<WorldState>();
		private static World[] _worlds = Array.Empty<World>();

		private static SimpleList<int> _freeIndexes = new SimpleList<int>();

		private static int _currentId = 0;

		public static WorldState CurrentWorldState
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _currentWorldState;
		}

		public static World CurrentWorld
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _currentWorld;
		}

		public static WorldId CurrentWorldId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _currentWorldState.WorldId;
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
		public static void Initialize()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
		}

#if UNITY_EDITOR
		private static void PlayModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state != UnityEditor.PlayModeStateChange.EnteredEditMode)
				return;

			UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;

			UnityEngine.Debug.Log($"{nameof(WorldManager)}.{nameof(Dispose)}");
			Dispose();
		}
#endif

		public static void Dispose()
		{
			for (var i = 0; i < _worlds.Length; i++)
			{
				if (_worlds[i] == null || !_worlds[i].IsValid)
					continue;
				_worlds[i].Dispose();
			}

			foreach (var worldState in _worldsStates)
			{
				worldState.Dispose();
			}

			_versions = Array.Empty<ushort>();
			_worldsStates = Array.Empty<WorldState>();
			_worlds = Array.Empty<World>();
			_freeIndexes.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this World world)
		{
			return world.worldState.GetWorldScope();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this WorldState worldState)
		{
			return worldState.WorldId.GetWorldScope(out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this WorldId worldId)
		{
			return worldId.GetWorldScope(out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this WorldId worldId, out WorldState worldState)
		{
			var scope = new WorldScope(_currentWorldState.IsValid && _currentWorldState.WorldId.IsValid() ? CurrentWorldId : default);
			worldId.SetCurrentWorld();
			worldState = _currentWorldState;
			return scope;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<World> GetAllWorlds()
		{
			foreach (var world in _worlds)
			{
				if (world == null)
					continue;

				yield return world;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetCurrentWorld(this ref WorldId worldId)
		{
			SetCurrentWorld(worldId.GetWorld(), worldId.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetCurrentWorld(WorldId worldId)
		{
			if (!worldId.IsValid())
				return;
			SetCurrentWorld(worldId.GetWorld(), worldId.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetCurrentWorld(this World world)
		{
			var worldState = world?.worldState ?? default;
			E.ASSERT(worldState.IsValid);

			SetCurrentWorld(world, worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetCurrentWorld(World world, WorldState worldState)
		{
			_currentWorld = world;
			_currentWorldState = worldState;

			var context = worldState.IsValid ? worldState.WorldId : default;
			ServiceContext<WorldId>.SetContext(context);
		}

		public static WorldState DeserializeWorld(ref StreamBufferReader stream)
		{
			throw new NotImplementedException();
			var world = WorldState.Deserialize(ref stream);
			return world;
		}

		public static World CreateWorld(int initialSize = -1)
		{
			var worldId = AllocateWorldId();

			ref var worldState = ref _worldsStates[worldId.id];
			ref var world = ref _worlds[worldId.id];

			if (!worldState.IsValid) // Переиспользуем память
				worldState = new WorldState(worldId, initialSize);

			worldId.version = ++_versions[worldId.id];
			worldState.SetupNewWorldId(worldId);

			world = new World(worldState);
			return world;
		}

		private static WorldId AllocateWorldId()
		{
			if (_freeIndexes.Count == 0)
			{
				_freeIndexes.Add(_versions.Length);

				ArrayExt.Expand(ref _versions, _versions.Length + 1, FIRST_VERSION);
				ArrayExt.Expand(ref _worldsStates, _worldsStates.Length + 1);
				ArrayExt.Expand(ref _worlds, _worlds.Length + 1);
			}

			var id = _freeIndexes.RemoveLast();
			var version = _versions[id];
			return new WorldId(id, version);
		}

		public static void RemoveWorld(this WorldState world)
		{
			world.WorldId.RemoveWorld();
		}

		public static void RemoveWorld(this WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new ArgumentException($"{nameof(WorldId)} with such Id [id: {worldId.version}] doesn't exist.");

			_worldsStates[worldId.id].Reset();
			_worlds[worldId.id] = null;
			_versions[worldId.id]++;
			_freeIndexes.Add(worldId.id);

			if (_currentWorldState == _worldsStates[worldId.id])
				SetCurrentWorld(null, default(WorldState));

			ServiceContext<WorldId>.RemoveContext(worldId);

			worldId.version = _versions[worldId.id];
			_worldsStates[worldId.id].SetupNewWorldId(worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldState GetWorldState(WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worldsStates[worldId.id];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldState GetWorldState(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worldsStates[worldId.id];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static World GetWorld(WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worlds[worldId.id];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static World GetWorld(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worlds[worldId.id];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValid(this WorldId worldId)
		{
			var result = true;
			NoBurstIsValid(worldId, ref result);
			return result;
		}

#if UNITY_5_3_OR_NEWER
		[Unity.Burst.BurstDiscard]
#endif
		private static void NoBurstIsValid(WorldId worldId, ref bool result)
		{
			result = worldId.id < _versions.Length && _versions[worldId.id] == worldId.version;
		}
	}

	public readonly ref struct WorldScope
	{
		private readonly WorldId _previousWorld;

		public WorldScope(WorldId previousWorld)
		{
			_previousWorld = previousWorld;
		}

		public void Dispose()
		{
			WorldManager.SetCurrentWorld(_previousWorld);
		}
	}
}
