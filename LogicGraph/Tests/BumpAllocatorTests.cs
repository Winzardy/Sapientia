#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Memory;
using Sapientia.MemoryAllocator;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Фаза 1: тесты двух обёрток над <see cref="BumpHeader"/> — <see cref="RawBumpAllocator"/>
	/// (сырой нативный блок) и <see cref="WorldBumpAllocator"/> (блок из основного аллокатора, через MemPtr).
	/// Проверяют монотонность аллокаций, round-trip значения и — главное для allocator-пути —
	/// корректный резолв после переезда блока + смены версии мира (snapshot).
	/// </summary>
	public class BumpAllocatorTests
	{
		// ───────────────────────── RawBumpAllocator ─────────────────────────

		[Test]
		public void Raw_AllocReturnsMonotonicOffsets()
		{
			var arena = new RawBumpAllocator(256);
			try
			{
				ref var header = ref arena.Value;

				var o1 = header.MemAlloc(8);
				var o2 = header.MemAlloc(8);

				Assert.Less(o1.byteOffset, o2.byteOffset, "Смещения должны монотонно расти.");
				Assert.AreEqual(o1.byteOffset + 8, o2.byteOffset, "Второе смещение должно идти ровно за первым.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Raw_Serialize_RoundTrips()
		{
			var source = new RawBumpAllocator(256);
			ref var header = ref source.Value;
			ref var slot = ref header.MemAlloc<int>(out var offset);
			slot = 777;

			var writer = new StreamBufferWriter(512);
			source.Serialize(ref writer);
			var bytes = writer.ToArray();
			writer.Dispose();
			source.Dispose();

			var reader = new StreamBufferReader(bytes);
			var restored = RawBumpAllocator.Deserialize(ref reader);
			reader.Dispose();
			try
			{
				Assert.AreEqual(777, restored.Value.GetRef(offset), "Значение не пережило serialize/deserialize raw-арены.");
			}
			finally
			{
				restored.Dispose();
			}
		}

		// ───────────────────────── WorldBumpAllocator ─────────────────────────

		[Test]
		public void World_RoundTripsOneInt()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			try
			{
				var arena = new MemBumpAllocator(worldState, 256);

				ref var header = ref arena.GetValue(worldState);
				ref var slot = ref header.MemAlloc<int>(out var offset);
				slot = 99;

				Assert.AreEqual(99, arena.GetValue(worldState).GetRef(offset), "Значение не прочиталось обратно из allocator-арены.");

				arena.Dispose(worldState);
			}
			finally
			{
				worldState.Dispose();
			}
		}

		[Test]
		public void World_Dispose_InvalidatesHandle()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			try
			{
				var arena = new MemBumpAllocator(worldState, 64);
				Assert.IsTrue(arena.IsValid, "Свежесозданная арена должна быть валидна.");

				arena.Dispose(worldState);
				Assert.IsFalse(arena.IsValid, "После Dispose handle должен стать невалидным.");
			}
			finally
			{
				worldState.Dispose();
			}
		}

		[Test]
		public void World_ReResolvesAfterSnapshotMoveAndVersionBump()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			var disposed = false;
			try
			{
				var arena = new MemBumpAllocator(worldState, 256);
				ref var header = ref arena.GetValue(worldState);
				ref var slot = ref header.MemAlloc<int>(out var offset);
				slot = 1234;

				// Полный round-trip WorldState: зоны реально переезжают на новые адреса + version++.
				var writer = new StreamBufferWriter(1024);
				worldState.Serialize(ref writer);
				var bytes = writer.ToArray();
				writer.Dispose();

				worldState.Dispose();
				disposed = true;

				var reader = new StreamBufferReader(bytes);
				var restoredWorld = WorldManager.DeserializeWorld(ref reader).worldState;
				reader.Dispose();
				try
				{
					// Тот же handle (MemPtr стабилен), новый WorldState с переехавшей базой —
					// position-independent BumpHeader должен прочитать значение без re-resolve.
					Assert.AreEqual(1234, arena.GetValue(restoredWorld).GetRef(offset),
						"Значение не прочиталось после переезда блока и смены версии мира.");
				}
				finally
				{
					restoredWorld.Dispose();
				}
			}
			finally
			{
				if (!disposed)
					worldState.Dispose();
			}
		}
	}
}
#endif
