using System;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;
using Sapientia.ServiceManagement;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public static class WorldManager
	{
		private static World _currentWorld = default;

		private static World[] _worlds = Array.Empty<World>();
		private static int _count = 0;
		private static int _currentId = 0;

		public static World CurrentWorld
		{
			[INLINE(256)]
			get => _currentWorld;
		}

		public static WorldId CurrentWorldId
		{
			[INLINE(256)]
			get => _currentWorld.WorldId;
		}

		[INLINE(256)]
		public static WorldScope GetWorldScope(this ref WorldId worldId, out World world)
		{
			var scope = new WorldScope(_currentWorld.IsValid ? CurrentWorldId : default);
			worldId.SetCurrentWorld();
			world = _currentWorld;
			return scope;
		}

		[INLINE(256)]
		public static void SetCurrentWorld(this ref WorldId worldId)
		{
			SetCurrentWorld(worldId.GetWorld());
		}

		[INLINE(256)]
		public static void SetCurrentWorld(WorldId worldId)
		{
			if (!worldId.IsValid())
				return;
			SetCurrentWorld(worldId.GetWorld());
		}

		[INLINE(256)]
		public static void SetCurrentWorld(World world)
		{
			_currentWorld = world;

			var context = world.IsValid ? world.WorldId : default;
			ServiceContext<WorldId>.SetContext(context);
		}

		public static World DeserializeWorld(ref StreamBufferReader stream)
		{
			var world = World.Deserialize(ref stream);

			ref var worldId = ref world.WorldId;

			// На момент создания не должно существовать мира с таким же Id
			if (worldId.IsValid())
				throw new System.Exception("World with such Id already exists.");

			Prewarm(_count);

			worldId.index = (ushort)_count++;

			_worlds[worldId.index] = world;
			_currentId = _currentId.Max(worldId.id + 1);

			return world;
		}

		public static World CreateWorld(int initialSize = -1)
		{
			Prewarm(_count);

			var worldId = new WorldId((ushort)_count++, (ushort)++_currentId);
			ref var world = ref _worlds[worldId.index];

			if (world.IsValid)
			{
				// Переиспользуем память
				world.Reset(worldId);
			}
			else
			{
				world = new World(worldId, initialSize);
			}

			return world;
		}

		private static void Prewarm(int index)
		{
			if (index < _worlds.Length)
				return;
			ArrayExt.Expand(ref _worlds, index + 1);
		}

		public static void RemoveWorld(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new ArgumentException($"{nameof(WorldId)} with such Id [id: {worldId.id}] doesn't exist.");

			_worlds[worldId.index].Clear();

			if (_currentWorld == _worlds[worldId.index])
				SetCurrentWorld(default(World));

			if (_count > 1)
			{
				// Не освобождаем память, будем её переиспользовать
				// !!! Если её освободить, по по непонятной причине происходит краш !!!
				_worlds[worldId.index].Swap(ref _worlds[_count - 1]);
			}
			_count--;
		}

		[INLINE(256)]
		public static World GetWorld(WorldId worldId)
		{
			if (!worldId.IsValid())
				return default;
			return _worlds[worldId.index];
		}

		[INLINE(256)]
		public static World GetWorld(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				return default;
			return _worlds[worldId.index];
		}

		[INLINE(256)]
		public static bool IsValid(this ref WorldId worldId)
		{
			if (worldId.index < _count && _worlds[worldId.index].WorldId.id == worldId.id)
				return true;

			for (ushort i = 0; i < _count; i++)
			{
				if (_worlds[i].WorldId.id != worldId.id)
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
