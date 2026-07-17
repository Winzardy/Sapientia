#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M6-C: <see cref="NodeFunctionRegistry"/> — function-table нод по индексу <c>TypeId&lt;ILogicNode&gt;</c>.
	/// Реестр — <b>инстанс</b> (без <c>SharedStatic</c>): строится/диспозится локально, статик-протечки нет. Проверяем
	/// <b>managed-путь</b> (plain .NET, без Burst/IndexedTypes) через <see cref="NodeFunctionRegistry.Create"/>: хранилище
	/// + lookup по индексу + диспатч (реальное исполнение адаптера) + жизненный цикл (Dispose). Reflection-сборка
	/// (<see cref="NodeFunctionRegistry.Build"/> по детям <c>ILogicNode</c>) и Burst-таблица требуют инициализированного
	/// <see cref="IndexedTypes"/> — в EditMode <c>TypeId&lt;ILogicNode&gt;.Count</c> = 0 ⇒ под <see cref="Assert.Ignore(string)"/>.
	/// </summary>
	public class NodeFunctionRegistryTests
	{
		/// <summary>Хендл ячейки <paramref name="index"/> (как в CacheTests/NodeExecutionTests): офсет = index*sizeof(CacheLink).</summary>
		private static CacheHandler<long> H(int index)
		{
			return new CacheHandler<long>
			{
				cell = new PtrOffset<CacheLink>(index * TSize<CacheLink>.size),
			};
		}

		/// <summary>Ручная <see cref="InstanceCache"/> на <paramref name="cellCount"/> ячеек (valueOffset[i]=i*8).</summary>
		private static InstanceCache CreateCache(int cellCount)
		{
			var template = new UnsafeArray<CacheLink>(default, cellCount);
			for (var i = 0; i < cellCount; i++)
				template[i].valueOffset = new PtrOffset(i * sizeof(long));

			var cache = InstanceCache.Create(default, cellCount, cellCount * sizeof(long), template.ptr);
			template.Dispose();
			return cache;
		}

		/// <summary>Тело: In + addend → Out (как StubAdd в NodeExecutionTests).</summary>
		private struct StubLogicAdd : ILogicNode
		{
			public CacheHandler<long> input;
			public CacheHandler<long> output;
			public long addend;

			public void Execute(ref NodeContext ctx)
			{
				ref var cache = ref ctx.Cache();
				cache.Read(input, out var v);
				cache.Write(output, v + addend);
			}
		}

		/// <summary>Тело: -In → Out (второй тип для проверки диспатча по разным ordinal'ам).</summary>
		private struct StubLogicNeg : ILogicNode
		{
			public CacheHandler<long> input;
			public CacheHandler<long> output;

			public void Execute(ref NodeContext ctx)
			{
				ref var cache = ref ctx.Cache();
				cache.Read(input, out var v);
				cache.Write(output, -v);
			}
		}

		[Test]
		public void Registry_InjectedManagedRoundTrips()
		{
			// Таблица из адаптеров двух типов по ordinal'ам 0/1 (mirror-сборка — без Burst/IndexedTypes).
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubLogicAdd>(),
				NodeInvoker.GetManaged<StubLogicNeg>(),
			});

			// Блоб с двумя узлами; в static-слайс каждого кладём тело нужного типа (авто-бейк handle'ов — M9, тут вручную).
			var bp = StubBlueprint.Of(new StubNode(staticSize: 128), new StubNode(staticSize: 128));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			var cacheArr = new UnsafeArray<InstanceCache>(default, 1);
			try
			{
				var compiledPtr = arena.Value.GetPtr(offset);

				// node0 = Add (in=cell0, out=cell1, +100); node1 = Neg (in=cell2, out=cell3).
				compiledPtr.Value().GetStaticNodeSlice(0).Cast<StubLogicAdd>().Value() = new StubLogicAdd
				{
					input = H(0),
					output = H(1),
					addend = 100L,
				};
				compiledPtr.Value().GetStaticNodeSlice(1).Cast<StubLogicNeg>().Value() = new StubLogicNeg
				{
					input = H(2),
					output = H(3),
				};

				cacheArr[0] = CreateCache(4);
				cacheArr[0].Write(H(0), 5L);  // вход Add
				cacheArr[0].Write(H(2), 7L);  // вход Neg

				// ordinal 0 → Add
				var ctx0 = new NodeContext { compiled = compiledPtr, cache = cacheArr.ptr, nodeId = 0 };
				var fn0 = registry.GetManaged(0);
				fn0(ref ctx0);

				// ordinal 1 → Neg
				var ctx1 = new NodeContext { compiled = compiledPtr, cache = cacheArr.ptr, nodeId = 1 };
				var fn1 = registry.GetManaged(1);
				fn1(ref ctx1);

				Assert.IsTrue(cacheArr[0].Read(H(1), out var addResult), "Add-Out должен быть посчитан.");
				Assert.AreEqual(105L, addResult, "ordinal 0 = Add: 5 + 100.");
				Assert.IsTrue(cacheArr[0].Read(H(3), out var negResult), "Neg-Out должен быть посчитан.");
				Assert.AreEqual(-7L, negResult, "ordinal 1 = Neg: -7. Диспатч по индексу ведёт к нужному телу.");
			}
			finally
			{
				cacheArr[0].Dispose();
				cacheArr.Dispose();
				arena.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Registry_InjectedTableIsRetrievable()
		{
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubLogicAdd>(),
				NodeInvoker.GetManaged<StubLogicNeg>(),
			});
			try
			{
				Assert.IsTrue(registry.IsCreated);
				Assert.IsNotNull(registry.GetManaged(0), "ordinal 0 извлекается.");
				Assert.IsNotNull(registry.GetManaged(1), "ordinal 1 извлекается.");
			}
			finally
			{
				registry.Dispose();
			}
		}

		[Test]
		public void Registry_DisposeReleases()
		{
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubLogicAdd>() });
			Assert.IsTrue(registry.IsCreated);
			registry.Dispose();
			Assert.IsFalse(registry.IsCreated, "После Dispose реестр не создан.");
			Assert.DoesNotThrow(() => registry.Dispose(), "Dispose идемпотентен.");
		}

		[Test]
		public void Registry_BuildEmptyWhenNoTypesRegistered()
		{
			// В EditMode IndexedTypes не инициализирован ⇒ детей ILogicNode нет ⇒ пустая (но созданная) таблица, без падений.
			if (TypeId<ILogicNode>.Count != 0)
			{
				Assert.Ignore("ILogicNode-типы зарегистрированы — пустой-кейс не воспроизводится в этом окружении.");
				return;
			}

			var registry = NodeFunctionRegistry.Build();
			try
			{
				Assert.IsTrue(registry.IsCreated, "Build при пустом IndexedTypes даёт созданный, но пустой реестр.");
			}
			finally
			{
				registry.Dispose();
			}
		}

		[Test]
		public void Registry_BuildBuildsDenseTable()
		{
			// Реальная reflection-сборка по детям ILogicNode требует инициализированного IndexedTypes
			// (генератор регистрирует детей контекста). В EditMode TypeId<ILogicNode>.Count = 0 ⇒ отложено.
			var typeCount = TypeId<ILogicNode>.Count;
			if (typeCount == 0)
			{
				Assert.Ignore("ILogicNode-типы не зарегистрированы в EditMode (IndexedTypes не инициализирован) — сборка отложена.");
				return;
			}

			var registry = NodeFunctionRegistry.Build();
			try
			{
				Assert.IsTrue(registry.IsCreated);
				for (var ordinal = 0; ordinal < typeCount; ordinal++)
					Assert.IsNotNull(registry.GetManaged(ordinal), $"managed-адаптер для ordinal {ordinal} должен быть собран.");
			}
			finally
			{
				registry.Dispose();
			}
		}
	}
}
#endif
