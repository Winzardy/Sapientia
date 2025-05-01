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
		private static World _currentWorld = null;

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
			get => _currentWorld.worldId;
		}

		[INLINE(256)]
		public static WorldScope GetAllocatorScope(this ref WorldId worldId, out World world)
		{
			var scope = new WorldScope(_currentWorld == default ? default : CurrentWorldId);
			worldId.SetCurrentAllocator();
			world = _currentWorld;
			return scope;
		}

		[INLINE(256)]
		public static void SetCurrentAllocator(this ref WorldId worldId)
		{
			SetCurrentAllocator(worldId.GetWorld());
		}

		[INLINE(256)]
		public static void SetCurrentAllocator(WorldId worldId)
		{
			if (!worldId.IsValid())
				return;
			SetCurrentAllocator(worldId.GetWorld());
		}

		[INLINE(256)]
		public static void SetCurrentAllocator(World world)
		{
			_currentWorld = world;

			var context = world?.worldId ?? default;
			ServiceContext<WorldId>.SetContext(context);
		}

		public static World DeserializeAllocator(ref StreamBufferReader stream)
		{
			var world = World.Deserialize(ref stream);

			ref var worldId = ref world.worldId;

			// На момент создания не должно существовать мира с таким же Id
			if (worldId.IsValid())
				throw new System.Exception("World with such Id already exists.");

			Prewarm(_count);

			worldId.index = (ushort)_count++;

			_worlds[worldId.index] = world;
			_currentId = _currentId.Max(worldId.id + 1);

			return world;
		}

		public static World CreateAllocator(int initialSize = -1)
		{
			Prewarm(_count);

			var worldId = new WorldId((ushort)_count++, (ushort)++_currentId);
			ref var world = ref _worlds[worldId.index];

			if (world != null)
			{
				// Переиспользуем память
				world.Reset(worldId);
			}
			else
			{
				world = new World();
				world.Initialize(worldId, initialSize);
			}

			return world;
		}

		private static void Prewarm(int index)
		{
			if (index < _worlds.Length)
				return;
			ArrayExt.Expand(ref _worlds, index + 1);
		}

		public static void RemoveAllocator(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new ArgumentException($"{nameof(WorldId)} with such Id [id: {worldId.id}] doesn't exist.");

			if (_currentWorld == _worlds[worldId.index])
				SetCurrentAllocator(null);

			if (_count > 1)
			{
				// Не освобождаем память, будем её переиспользовать
				// !!! Если её освободить, по по непонятной причине происходит краш !!!
				ref var a = ref _worlds[worldId.index];
				ref var b = ref _worlds[_count - 1];

				(a, b) = (b, a);
			}
			_count--;
		}

		[INLINE(256)]
		public static World GetWorld(WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new ArgumentException($"{nameof(WorldId)} with such Id [id: {worldId.id}] doesn't exist.");
			return _worlds[worldId.index];
		}

		[INLINE(256)]
		public static World GetWorld(this ref WorldId worldId)
		{
			if (!worldId.IsValid())
				throw new ArgumentException($"{nameof(WorldId)} with such Id [id: {worldId.id}] doesn't exist.");
			return _worlds[worldId.index];
		}

		[INLINE(256)]
		public static bool IsValid(this ref WorldId worldId)
		{
			if (worldId.index < _count && _worlds[worldId.index].worldId.id == worldId.id)
				return true;

			for (ushort i = 0; i < _count; i++)
			{
				if (_worlds[i].worldId.id != worldId.id)
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
				SetCurrentAllocator(_previousWorld);
			}
		}
	}
}
