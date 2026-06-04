#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.MemoryAllocator;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Фаза 2: блоки инстанса (instance cache + instance persistent). Проверяют аллокацию/обнуление
	/// обоих блоков, reset-семантику (cache обнуляется, persistent выживает), чистоту zero-size и
	/// освобождение блоков на Dispose.
	/// </summary>
	public class InstanceScopeTests
	{
		[Test]
		public void InstanceScope_CreateAllocatesBothBlocks()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			var bp = StubBlueprint.Of(new StubNode(instanceCacheSize: 16, instancePersistentSize: 16));
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var instancePtr = BlueprintInstance.Create(worldState, compiled, 0);
				ref var instance = ref instancePtr.GetValue(worldState);

				Assert.IsTrue(instance.instanceCache.IsValid(), "Блок instance cache не выделен.");
				Assert.IsTrue(instance.instancePersistent.IsValid(), "Блок instance persistent не выделен.");
				// Свежие блоки обнулены.
				Assert.AreEqual(0, instance.instanceCache.GetPtr(worldState).Value<long>(), "instance cache не обнулён.");
				Assert.AreEqual(0, instance.instancePersistent.GetPtr(worldState).Value<long>(), "instance persistent не обнулён.");

				instance.Dispose(worldState);
				instancePtr.Dispose(worldState);
			}
			finally
			{
				arena.Dispose();
				worldState.Dispose();
			}
		}

		[Test]
		public void InstanceScope_ResetCacheClearsCacheKeepsPersistent()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			var bp = StubBlueprint.Of(new StubNode(instanceCacheSize: 16, instancePersistentSize: 16));
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var instancePtr = BlueprintInstance.Create(worldState, compiled, 0);
				ref var instance = ref instancePtr.GetValue(worldState);

				// Заполняем оба блока целиком (по 16 байт = 2 long), включая последние байты.
				var cache = instance.instanceCache.GetPtr(worldState);
				var persistent = instance.instancePersistent.GetPtr(worldState);
				cache.Value<long>() = 111;
				(cache + 8).Value<long>() = 112;
				persistent.Value<long>() = 221;
				(persistent + 8).Value<long>() = 222;

				instance.ResetCache(worldState, compiled);

				Assert.AreEqual(0, cache.Value<long>(), "Cache[0] не обнулился после ResetCache.");
				Assert.AreEqual(0, (cache + 8).Value<long>(), "Cache[8] (последние байты) не обнулился.");
				Assert.AreEqual(221, persistent.Value<long>(), "Persistent[0] затёрт ResetCache.");
				Assert.AreEqual(222, (persistent + 8).Value<long>(), "Persistent[8] затёрт ResetCache.");

				instance.Dispose(worldState);
				instancePtr.Dispose(worldState);
			}
			finally
			{
				arena.Dispose();
				worldState.Dispose();
			}
		}

		[Test]
		public void InstanceScope_ZeroSizeInstanceBlocksClean()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			// instance-области нулевые (нода занимает только static).
			var bp = StubBlueprint.Of(new StubNode(staticSize: 8));
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var instancePtr = BlueprintInstance.Create(worldState, compiled, 0);
				ref var instance = ref instancePtr.GetValue(worldState);

				Assert.IsFalse(instance.instanceCache.IsValid(), "Нулевой instance cache не должен выделяться.");
				Assert.IsFalse(instance.instancePersistent.IsValid(), "Нулевой instance persistent не должен выделяться.");

				// Reset/Dispose на пустых блоках — без ассертов.
				instance.ResetCache(worldState, compiled);
				instance.Dispose(worldState);
				instancePtr.Dispose(worldState);
			}
			finally
			{
				arena.Dispose();
				worldState.Dispose();
			}
		}

		[Test]
		public void InstanceScope_DisposeFreesBlocks()
		{
			var worldState = WorldManager.CreateWorld(1024).worldState;
			var bp = StubBlueprint.Of(new StubNode(instanceCacheSize: 16, instancePersistentSize: 16));
			var arena = CompiledBlueprint.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var instancePtr = BlueprintInstance.Create(worldState, compiled, 0);
				ref var instance = ref instancePtr.GetValue(worldState);

				Assert.IsTrue(instance.instanceCache.IsValid());
				Assert.IsTrue(instance.instancePersistent.IsValid());

				instance.Dispose(worldState);

				Assert.IsFalse(instance.instanceCache.IsValid(), "После Dispose instance cache должен стать невалидным.");
				Assert.IsFalse(instance.instancePersistent.IsValid(), "После Dispose instance persistent должен стать невалидным.");

				instancePtr.Dispose(worldState);
			}
			finally
			{
				arena.Dispose();
				worldState.Dispose();
			}
		}
	}
}
#endif
