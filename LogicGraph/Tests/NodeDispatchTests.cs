#if UNITY_5_4_OR_NEWER
using System;
using NUnit.Framework;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M6-F: <b>интеграция диспетчера</b> — прогон <see cref="ExecutionGraph.Drain"/>-порядка через
	/// <see cref="NodeInvoker.Run"/>/<see cref="NodeInvoker.Invoke"/>: резолв памяти инстанса из
	/// <see cref="ExecutionScope"/> (Cache + Persistence) + dispatch через <see cref="ExecutionScope.Registry"/>.
	/// Тела нод <b>реально исполняются</b> managed-путём (реестр строим через
	/// <see cref="NodeFunctionRegistry.Create(ExecuteFn[], bool)"/> с <c>forceManaged: true</c> ⇒ Burst/<c>IndexedTypes</c>
	/// не нужны — plain managed-семантика). Прогон — через Unity Test Runner (весь файл под <c>#if UNITY_5_4_OR_NEWER</c>,
	/// как все тесты LogicGraph); покрытие <b>реальное</b>, под <see cref="Assert.Ignore(string)"/> — ничего.
	///
	/// <para>Авто-бейк port-handle'ов в тело (из Map) — M9; здесь тело кладём в static-слайс вручную, а Cache-handle'ы
	/// берём <b>из Map блоба</b> (<see cref="CompiledBlueprintHeader.GetNodeInOut"/> → <see cref="RegionPtr.cacheData"/>),
	/// без хардкода ordinal'ов. ordinal реестра задаётся явным <c>StubNode(typeId:)</c>.</para>
	/// </summary>
	public class NodeDispatchTests
	{
		// ─────────────────────────── Тела нод (managed-исполнение) ───────────────────────────

		/// <summary>Корень: пишет константу <see cref="seed"/> в свой Cache-Out.</summary>
		private struct StubSeed : ILogicNode
		{
			public CacheHandler<long> output;
			public long seed;

			public void Execute(ref NodeContext ctx)
			{
				ctx.Cache().Write(output, seed);
			}
		}

		/// <summary>Читает In из Cache, пишет In + <see cref="addend"/> в Out.</summary>
		private struct StubAdd : ILogicNode
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

		/// <summary>Join: читает два входа и пишет их сумму в Out (доказывает, что оба предшественника исполнены раньше).</summary>
		private struct StubSum2 : ILogicNode
		{
			public CacheHandler<long> inputA;
			public CacheHandler<long> inputB;
			public CacheHandler<long> output;

			public void Execute(ref NodeContext ctx)
			{
				ref var cache = ref ctx.Cache();
				cache.Read(inputA, out var a);
				cache.Read(inputB, out var b);
				cache.Write(output, a + b);
			}
		}

		/// <summary>Инкрементит per-instance long в Persistence (постоянный стейт, переживает reset кеша).</summary>
		private struct StubPersistInc : ILogicNode
		{
			public void Execute(ref NodeContext ctx)
			{
				ref var counter = ref ctx.PersistenceSlice().Cast<long>().Value();
				counter = counter + 1;
			}
		}

		/// <summary>Читает per-instance seed из Persistence и публикует его в Cache-Out (для проверки изоляции памяти инстансов).</summary>
		private struct StubSeedFromPersistence : ILogicNode
		{
			public CacheHandler<long> output;

			public void Execute(ref NodeContext ctx)
			{
				var v = ctx.PersistenceSlice().Cast<long>().Value();
				ctx.Cache().Write(output, v);
			}
		}

		// ─────────────────────────── Хелперы ───────────────────────────

		/// <summary>Cache-handle порта <paramref name="portIndex"/> ноды (In'ы, затем Out'ы) из Map блоба:
		/// <see cref="RegionPtr.cacheData"/> — ordinal ячейки. Вызывать через <c>ref</c> блоба (резолв self-relative Map).</summary>
		private static CacheHandler<long> PortHandle(ref CompiledBlueprintHeader compiled, int nodeId, int portIndex)
		{
			var port = (compiled.GetNodeInOut(nodeId).Cast<RegionPtr>() + portIndex).Value();
			int ordinal = port.cacheData; // implicit Id<CacheLink> → int (no-explicit-id-int-cast)
			return new CacheHandler<long>
			{
				cell = new PtrOffset<CacheLink>(ordinal * TSize<CacheLink>.size),
			};
		}

		/// <summary>Пишет <paramref name="value"/> в Persistence-слайс ноды <paramref name="nodeId"/> инстанса (до прогона).</summary>
		private static void SeedPersistence(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled, BlueprintInstanceId id, int nodeId, long value)
		{
			ref var persistence = ref scope.GetInstancePersistent(id);
			(persistence.GetPtr() + compiled.GetNodePersistenceOffset(nodeId).byteOffset).Cast<long>().Value() = value;
		}

		// ─────────────────────────── Тесты ───────────────────────────

		[Test]
		public void Invoke_SingleNode_ResolvesMemoryAndDispatches()
		{
			// Одна нода читает Persistence (seed=42) и пишет в Cache-Out. Прямой Invoke (без Run): проверяет, что
			// NodeContext собран из памяти инстанса (Persistence + Cache) и диспатч прошёл через scope.Registry.
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubSeedFromPersistence>() }, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var graph = ExecutionGraph.Create();
			try
			{
				var bp = StubBlueprint.Of(new StubNode(staticSize: 64, cacheSize: 16, persistanceSize: 8,
					outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 0));
				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);

				var outHandle = PortHandle(ref compiled, 0, 0);
				compiled.GetStaticNodeSlice(0).Cast<StubSeedFromPersistence>().Value() = new StubSeedFromPersistence { output = outHandle };
				SeedPersistence(ref scope, ref compiled, id, 0, 42L);

				NodeInvoker.Invoke(ref scope, ref compiled, new NodeInstanceId { blueprintId = id, nodeId = 0 });

				Assert.IsTrue(scope.GetInstanceCache(id).Read(outHandle, out var result), "Out посчитан через per-node seam.");
				Assert.AreEqual(42L, result, "seam резолвит Persistence (42) + Cache из scope и диспатчит ноду.");
			}
			finally
			{
				graph.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_Chain_ExecutesInDependencyOrder()
		{
			// A(seed 10) → B(+5) → C(+5): после Run C-Out = 20. Доказывает порядок зависимостей + проброс Cache по цепочке.
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubSeed>(),
				NodeInvoker.GetManaged<StubAdd>(),
				NodeInvoker.GetManaged<StubAdd>(),
			}, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var graph = ExecutionGraph.Create();
			try
			{
				var aOut = new NodeOutput<long>();
				var bIn = new NodeInput<long>();
				var bOut = new NodeOutput<long>();
				var cIn = new NodeInput<long>();
				var bp = StubBlueprint.Of(
					new StubNode(staticSize: 64, cacheSize: 16, outputs: new NodeOutput[] { aOut }, typeId: 0),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { bOut }, typeId: 1),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { cIn }, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 2));
				bp.inputToOutput[bIn] = aOut;
				bp.inputToOutput[cIn] = bOut;

				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);

				compiled.GetStaticNodeSlice(0).Cast<StubSeed>().Value() = new StubSeed { output = PortHandle(ref compiled, 0, 0), seed = 10L };
				compiled.GetStaticNodeSlice(1).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 1, 0), output = PortHandle(ref compiled, 1, 1), addend = 5L };
				compiled.GetStaticNodeSlice(2).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 2, 0), output = PortHandle(ref compiled, 2, 1), addend = 5L };

				graph.Inject(ref compiled, id);

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[compiled.NodesCount];
				var n = NodeInvoker.Run(ref scope, ref compiled, ref graph, order);
				Assert.AreEqual(3, n, "Все три ноды исполнены.");

				var cOut = PortHandle(ref compiled, 2, 1);
				Assert.IsTrue(scope.GetInstanceCache(id).Read(cOut, out var result), "C-Out посчитан.");
				Assert.AreEqual(20L, result, "10 → +5 → +5 по цепочке в порядке зависимостей.");
			}
			finally
			{
				graph.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_Diamond_JoinSeesBothBranches()
		{
			// A(10) → B(+1),C(+2); D = B + C. После Run D-Out = 11+12 = 23 ⇒ D исполнена после обеих ветвей.
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubSeed>(),
				NodeInvoker.GetManaged<StubAdd>(),
				NodeInvoker.GetManaged<StubAdd>(),
				NodeInvoker.GetManaged<StubSum2>(),
			}, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var graph = ExecutionGraph.Create();
			try
			{
				var aOut = new NodeOutput<long>();
				var bIn = new NodeInput<long>();
				var bOut = new NodeOutput<long>();
				var cIn = new NodeInput<long>();
				var cOut = new NodeOutput<long>();
				var dInB = new NodeInput<long>();
				var dInC = new NodeInput<long>();
				var bp = StubBlueprint.Of(
					new StubNode(staticSize: 64, cacheSize: 16, outputs: new NodeOutput[] { aOut }, typeId: 0),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { bOut }, typeId: 1),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { cIn }, outputs: new NodeOutput[] { cOut }, typeId: 2),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { dInB, dInC }, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 3));
				bp.inputToOutput[bIn] = aOut;
				bp.inputToOutput[cIn] = aOut;
				bp.inputToOutput[dInB] = bOut;
				bp.inputToOutput[dInC] = cOut;

				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);

				compiled.GetStaticNodeSlice(0).Cast<StubSeed>().Value() = new StubSeed { output = PortHandle(ref compiled, 0, 0), seed = 10L };
				compiled.GetStaticNodeSlice(1).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 1, 0), output = PortHandle(ref compiled, 1, 1), addend = 1L };
				compiled.GetStaticNodeSlice(2).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 2, 0), output = PortHandle(ref compiled, 2, 1), addend = 2L };
				compiled.GetStaticNodeSlice(3).Cast<StubSum2>().Value() = new StubSum2 { inputA = PortHandle(ref compiled, 3, 0), inputB = PortHandle(ref compiled, 3, 1), output = PortHandle(ref compiled, 3, 2) };

				graph.Inject(ref compiled, id);

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[compiled.NodesCount];
				Assert.AreEqual(4, NodeInvoker.Run(ref scope, ref compiled, ref graph, order), "Все четыре ноды исполнены.");

				var dOut = PortHandle(ref compiled, 3, 2);
				Assert.IsTrue(scope.GetInstanceCache(id).Read(dOut, out var result), "D-Out посчитан (join увидел обе ветви).");
				Assert.AreEqual(23L, result, "D = (10+1) + (10+2) = 23 ⇒ D исполнена после B и C.");
			}
			finally
			{
				graph.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_MultiInstanceSameBlueprint_IndependentMemory()
		{
			// Один блюпринт (нода читает per-instance Persistence seed → Cache-Out), два инстанса с разными seed'ами.
			// Один Run гоняет оба; результаты различны ⇒ память резолвится по NodeInstanceId.blueprintId (изоляция).
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubSeedFromPersistence>() }, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var graph = ExecutionGraph.Create();
			try
			{
				var bp = StubBlueprint.Of(new StubNode(staticSize: 64, cacheSize: 16, persistanceSize: 8,
					outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 0));
				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id1 = scope.CreateInstance(ref storage, key);
				var id2 = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);

				var outHandle = PortHandle(ref compiled, 0, 0);
				compiled.GetStaticNodeSlice(0).Cast<StubSeedFromPersistence>().Value() = new StubSeedFromPersistence { output = outHandle };

				SeedPersistence(ref scope, ref compiled, id1, 0, 10L);
				SeedPersistence(ref scope, ref compiled, id2, 0, 20L);

				graph.Inject(ref compiled, id1);
				graph.Inject(ref compiled, id2);

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[2];
				Assert.AreEqual(2, NodeInvoker.Run(ref scope, ref compiled, ref graph, order), "Обе ноды (по одной на инстанс) исполнены.");

				Assert.IsTrue(scope.GetInstanceCache(id1).Read(outHandle, out var r1));
				Assert.IsTrue(scope.GetInstanceCache(id2).Read(outHandle, out var r2));
				Assert.AreEqual(10L, r1, "Инстанс 1 видит свой Persistence seed.");
				Assert.AreEqual(20L, r2, "Инстанс 2 видит свой Persistence seed (память инстансов независима).");
			}
			finally
			{
				graph.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_PersistenceNode_PersistsAcrossRuns()
		{
			// Нода инкрементит Persistence-счётчик. Два Run'а ⇒ counter == 2: Persistence проброшен через scope и
			// переживает reset кеша (ResetAllCache не трогает Persistence).
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubPersistInc>() }, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var graph = ExecutionGraph.Create();
			try
			{
				var bp = StubBlueprint.Of(new StubNode(staticSize: 8, persistanceSize: 8, typeId: 0));
				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);
				graph.Inject(ref compiled, id);

				Span<NodeInstanceId> order = stackalloc NodeInstanceId[compiled.NodesCount];
				NodeInvoker.Run(ref scope, ref compiled, ref graph, order);
				NodeInvoker.Run(ref scope, ref compiled, ref graph, order);

				var counter = (scope.GetInstancePersistent(id).GetPtr() + compiled.GetNodePersistenceOffset(0).byteOffset).Cast<long>().Value();
				Assert.AreEqual(2L, counter, "Persistence пережил два прогона (++ дважды), reset кеша его не сбросил.");
			}
			finally
			{
				graph.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_ResetsCacheEachRun_Deterministic()
		{
			// Два Run'а одного графа (cache-only) дают побитово равный результат: ResetAllCache на старте + детерминизм.
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubSeed>(),
				NodeInvoker.GetManaged<StubAdd>(),
			}, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var graph = ExecutionGraph.Create();
			try
			{
				var aOut = new NodeOutput<long>();
				var bIn = new NodeInput<long>();
				var bp = StubBlueprint.Of(
					new StubNode(staticSize: 64, cacheSize: 16, outputs: new NodeOutput[] { aOut }, typeId: 0),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 1));
				bp.inputToOutput[bIn] = aOut;

				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);
				compiled.GetStaticNodeSlice(0).Cast<StubSeed>().Value() = new StubSeed { output = PortHandle(ref compiled, 0, 0), seed = 7L };
				compiled.GetStaticNodeSlice(1).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 1, 0), output = PortHandle(ref compiled, 1, 1), addend = 3L };
				graph.Inject(ref compiled, id);

				var bOutHandle = PortHandle(ref compiled, 1, 1);
				Span<NodeInstanceId> order = stackalloc NodeInstanceId[compiled.NodesCount];

				NodeInvoker.Run(ref scope, ref compiled, ref graph, order);
				scope.GetInstanceCache(id).Read(bOutHandle, out var first);

				NodeInvoker.Run(ref scope, ref compiled, ref graph, order);
				scope.GetInstanceCache(id).Read(bOutHandle, out var second);

				Assert.AreEqual(10L, first, "7 + 3.");
				Assert.AreEqual(first, second, "Повторный Run после reset кеша детерминирован (побитово равен).");
			}
			finally
			{
				graph.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}
	}
}
#endif
