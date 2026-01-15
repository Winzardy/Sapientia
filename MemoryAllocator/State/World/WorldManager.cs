using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.Memory;
using Sapientia.ServiceManagement;

namespace Sapientia.MemoryAllocator
{
	public static class WorldManager
	{
		private static WorldState _currentWorldState = default;
		private static World _currentWorld = null;

		private static WorldState[] _worldsStates = Array.Empty<WorldState>();
		private static World[] _worlds = Array.Empty<World>();
		private static int _count = 0;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this World world)
		{
			return world.worldState.GetWorldScope();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this ref WorldState worldState)
		{
			return worldState.WorldId.GetWorldScope(out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this ref WorldId worldId)
		{
			return worldId.GetWorldScope(out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldScope GetWorldScope(this ref WorldId worldId, out WorldState worldState)
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
			var world = WorldState.Deserialize(ref stream);

			ref var worldId = ref world.WorldId;

			// На момент создания не должно существовать мира с таким же Id
			if (worldId.IsValid())
				throw new System.Exception("World with such Id already exists.");

			Prewarm(_count);

			worldId.index = (ushort) _count++;

			_worldsStates[worldId.index] = world;
			_currentId = _currentId.Max(worldId.id + 1);

			return world;
		}

		public static World CreateWorld(int initialSize = -1)
		{
			Prewarm(_count);

			var worldId = new WorldId(_count++, ++_currentId);
			ref var worldState = ref _worldsStates[worldId.index];
			ref var world = ref _worlds[worldId.index];

			if (worldState.IsValid)
			{
				// Переиспользуем память
				worldState.SetupNewWorldId(worldId);
			}
			else
			{
				worldState = new WorldState(worldId, initialSize);
			}

			world = new World(worldState);
			return world;
		}

		private static void Prewarm(int index)
		{
			if (index >= _worlds.Length)
				ArrayExt.Expand(ref _worlds, index + 1);
			if (index >= _worldsStates.Length)
				ArrayExt.Expand(ref _worldsStates, index + 1);
		}

		public static void RemoveWorld(this ref WorldState world)
		{
			world.WorldId.RemoveWorld();
		}

		public static void RemoveWorld(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new ArgumentException($"{nameof(WorldId)} with such Id [id: {worldId.id}] doesn't exist.");

			_worldsStates[worldId.index].Reset();

			if (_currentWorldState == _worldsStates[worldId.index])
				SetCurrentWorld(null, default(WorldState));

			if (_count > 1)
			{
				// Не освобождаем память, будем её переиспользовать
				// !!! Если её освободить, по непонятной причине происходит краш !!!
				ValuesExt.Swap(ref _worlds[worldId.index], ref _worlds[_count - 1]);
				_worldsStates[worldId.index].Swap(ref _worldsStates[_count - 1]);

				_worlds[_count - 1] = null;
			}

			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldState GetWorldState(WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worldsStates[worldId.index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WorldState GetWorldState(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worldsStates[worldId.index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static World GetWorld(WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worlds[worldId.index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static World GetWorld(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new Exception($"World with such Id [id: {worldId}] is invalid.");
			return _worlds[worldId.index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValid(this ref WorldId worldId)
		{
			if (worldId.index < _count && _worldsStates[worldId.index].IsValid && _worldsStates[worldId.index].WorldId.id == worldId.id)
				return true;

			for (ushort i = 0; i < _count; i++)
			{
				if (_worldsStates[i].WorldId.id != worldId.id)
					continue;
				worldId.index = i;
				return true;
			}

			return false;
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
				SetCurrentWorld(_previousWorld);
			}
		}
	}
}
