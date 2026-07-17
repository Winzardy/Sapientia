#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M7-B: <b>прогон через демандный оркестратор</b> — <see cref="Orchestrator.Inject"/> сидит входы,
	/// <see cref="Orchestrator.Run"/> исполняет граф в порядке зависимостей (ready-по-предшественникам + push +
	/// мемоизация) через <see cref="NodeInvoker.Invoke"/>: резолв памяти инстанса из <see cref="ExecutionScope"/>
	/// (Cache + Persistence) + dispatch через <see cref="ExecutionScope.Registry"/>.
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

		/// <summary>Источник: пишет <see cref="seed"/> в Cache-Out И инкрементит Persistence-счётчик исполнений
		/// (детектор повторного запуска — для проверки мемоизации общего предка).</summary>
		private struct StubSeedCounting : ILogicNode
		{
			public CacheHandler<long> output;
			public long seed;

			public void Execute(ref NodeContext ctx)
			{
				ref var counter = ref ctx.PersistenceSlice().Cast<long>().Value();
				counter = counter + 1;
				ctx.Cache().Write(output, seed);
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

		/// <summary>Входная нода прогона: <see cref="NodeInstanceId"/> = (инстанс, <paramref name="nodeId"/>). Inject строит подграф от неё.</summary>
		private static NodeInstanceId Entry(BlueprintInstanceId id, Id<NodeHeader> nodeId)
		{
			return new NodeInstanceId { blueprintId = id, nodeId = nodeId };
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
			var orch = Orchestrator.Create();
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
				orch.Dispose();
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
			var orch = Orchestrator.Create();
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

				orch.Inject(new[] { Entry(id, 0) });

				var n = orch.Run(ref scope, ref storage);
				Assert.AreEqual(3, n, "Все три ноды исполнены.");

				var cOut = PortHandle(ref compiled, 2, 1);
				Assert.IsTrue(scope.GetInstanceCache(id).Read(cOut, out var result), "C-Out посчитан.");
				Assert.AreEqual(20L, result, "10 → +5 → +5 по цепочке в порядке зависимостей.");
			}
			finally
			{
				orch.Dispose();
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
			var orch = Orchestrator.Create();
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

				orch.Inject(new[] { Entry(id, 0) });

				Assert.AreEqual(4, orch.Run(ref scope, ref storage), "Все четыре ноды исполнены.");

				var dOut = PortHandle(ref compiled, 3, 2);
				Assert.IsTrue(scope.GetInstanceCache(id).Read(dOut, out var result), "D-Out посчитан (join увидел обе ветви).");
				Assert.AreEqual(23L, result, "D = (10+1) + (10+2) = 23 ⇒ D исполнена после B и C.");
			}
			finally
			{
				orch.Dispose();
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
			var orch = Orchestrator.Create();
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

				orch.Inject(new[] { Entry(id1, 0), Entry(id2, 0) });

				Assert.AreEqual(2, orch.Run(ref scope, ref storage), "Обе ноды (по одной на инстанс) исполнены.");

				Assert.IsTrue(scope.GetInstanceCache(id1).Read(outHandle, out var r1));
				Assert.IsTrue(scope.GetInstanceCache(id2).Read(outHandle, out var r2));
				Assert.AreEqual(10L, r1, "Инстанс 1 видит свой Persistence seed.");
				Assert.AreEqual(20L, r2, "Инстанс 2 видит свой Persistence seed (память инстансов независима).");
			}
			finally
			{
				orch.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_PersistenceNode_PersistsAcrossRuns()
		{
			// Нода инкрементит Persistence-счётчик. Два Run'а ⇒ counter == 2: Persistence проброшен через scope и
			// живёт между прогонами (он — постоянный per-instance стейт, сброс кеша его в принципе не трогает).
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubPersistInc>() }, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var orch = Orchestrator.Create();
			try
			{
				var bp = StubBlueprint.Of(new StubNode(staticSize: 8, persistanceSize: 8, typeId: 0));
				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);
				// Два прогона: Inject перед каждым (Run опустошает очередь).
				orch.Inject(new[] { Entry(id, 0) });
				orch.Run(ref scope, ref storage);
				orch.Inject(new[] { Entry(id, 0) });
				orch.Run(ref scope, ref storage);

				var counter = (scope.GetInstancePersistent(id).GetPtr() + compiled.GetNodePersistenceOffset(0).byteOffset).Cast<long>().Value();
				Assert.AreEqual(2L, counter, "Persistence пережил два прогона (++ дважды), reset кеша его не сбросил.");
			}
			finally
			{
				orch.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_CallerResetsCachePerIteration_Deterministic()
		{
			// Сброс кеша — на стороне ВЫЗЫВАЮЩЕГО раз за итерацию (не в Run). Две итерации (reset → Run) одного
			// графа (cache-only) дают побитово равный результат: детерминизм при caller-driven сбросе.
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubSeed>(),
				NodeInvoker.GetManaged<StubAdd>(),
			}, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var orch = Orchestrator.Create();
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
				var bOutHandle = PortHandle(ref compiled, 1, 1);
				var template = compiled.GetCacheCellsTemplate();

				// Итерация 1: вызывающий сбрасывает кеш, сидит вход, Run.
				scope.ResetInstanceCache(id, template);
				orch.Inject(new[] { Entry(id, 0) });
				orch.Run(ref scope, ref storage);
				scope.GetInstanceCache(id).Read(bOutHandle, out var first);

				// Итерация 2: снова reset → Inject → Run.
				scope.ResetInstanceCache(id, template);
				orch.Inject(new[] { Entry(id, 0) });
				orch.Run(ref scope, ref storage);
				scope.GetInstanceCache(id).Read(bOutHandle, out var second);

				Assert.AreEqual(10L, first, "7 + 3.");
				Assert.AreEqual(first, second, "Детерминизм при сбросе кеша вызывающим раз за итерацию (побитово равен).");
			}
			finally
			{
				orch.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_NoInject_ReturnsZero()
		{
			// Run без Inject: work-list пуст → цикл не исполняется → 0 нод (без падений на пустом графе).
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubSeed>() }, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var orch = Orchestrator.Create();
			try
			{
				var bp = StubBlueprint.Of(new StubNode(staticSize: 8, typeId: 0));
				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());
				ref var compiled = ref storage.Get(key);

				Assert.AreEqual(0, orch.Run(ref scope, ref storage), "Без Inject work-list пуст — 0 нод.");
			}
			finally
			{
				orch.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_SharedAncestor_RunsOnce()
		{
			// A (источник, инкрементит Persistence-счётчик) → B, C. Входы = [B, C]; оба ТЯНУТ A,
			// но A исполняется РОВНО ОДИН раз (мемоизация по computed-биту). Доказывает pull + дедуп + memoize.
			var registry = NodeFunctionRegistry.Create(new[]
			{
				NodeInvoker.GetManaged<StubSeedCounting>(),
				NodeInvoker.GetManaged<StubAdd>(),
				NodeInvoker.GetManaged<StubAdd>(),
			}, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var orch = Orchestrator.Create();
			try
			{
				var aOut = new NodeOutput<long>();
				var bIn = new NodeInput<long>();
				var cIn = new NodeInput<long>();
				var bp = StubBlueprint.Of(
					new StubNode(staticSize: 64, cacheSize: 16, persistanceSize: 8, outputs: new NodeOutput[] { aOut }, typeId: 0),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { bIn }, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 1),
					new StubNode(staticSize: 64, cacheSize: 16, inputs: new NodeInput[] { cIn }, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 2));
				bp.inputToOutput[bIn] = aOut;
				bp.inputToOutput[cIn] = aOut;

				var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				ref var compiled = ref storage.Get(key);
				compiled.GetStaticNodeSlice(0).Cast<StubSeedCounting>().Value() = new StubSeedCounting { output = PortHandle(ref compiled, 0, 0), seed = 5L };
				compiled.GetStaticNodeSlice(1).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 1, 0), output = PortHandle(ref compiled, 1, 1), addend = 1L };
				compiled.GetStaticNodeSlice(2).Cast<StubAdd>().Value() = new StubAdd { input = PortHandle(ref compiled, 2, 0), output = PortHandle(ref compiled, 2, 1), addend = 2L };

				orch.Inject(new[] { Entry(id, 1), Entry(id, 2) }); // входы = B и C, тянут общего предка A
				var n = orch.Run(ref scope, ref storage);

				var counter = (scope.GetInstancePersistent(id).GetPtr() + compiled.GetNodePersistenceOffset(0).byteOffset).Cast<long>().Value();
				Assert.AreEqual(1L, counter, "Общий предок A исполнен ровно один раз (мемоизация), хотя его тянут и B, и C.");
				Assert.AreEqual(3, n, "Исполнены A, B, C по разу.");
				Assert.IsTrue(scope.GetInstanceCache(id).Read(PortHandle(ref compiled, 1, 1), out var bOut));
				Assert.AreEqual(6L, bOut, "B = A(5) + 1.");
				Assert.IsTrue(scope.GetInstanceCache(id).Read(PortHandle(ref compiled, 2, 1), out var cOut));
				Assert.AreEqual(7L, cOut, "C = A(5) + 2.");
			}
			finally
			{
				orch.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}

		[Test]
		public void Run_MultiBlueprint_IndependentResults()
		{
			// Два РАЗНЫХ блюпринта (ключи (1,1) и (2,1)) в одном Run: входы обоих в очереди, оркестратор резолвит
			// compiled per-instance (M7-E). bp1 сидит 100, bp2 — 200; результаты независимы.
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubSeed>() }, forceManaged: true);
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create(registry: registry);
			var orch = Orchestrator.Create();
			try
			{
				var bp1 = StubBlueprint.Of(1, 1, new StubNode(staticSize: 64, cacheSize: 16, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 0));
				var bp2 = StubBlueprint.Of(2, 1, new StubNode(staticSize: 64, cacheSize: 16, outputs: new NodeOutput[] { new NodeOutput<long>() }, typeId: 0));

				var arena1 = BlueprintCompiler.CompileLayout(bp1, out var off1);
				var key1 = arena1.Value.GetValue(off1).blueprintKey;
				storage.Add(arena1, off1, CompiledEnvironment.Compile());
				var arena2 = BlueprintCompiler.CompileLayout(bp2, out var off2);
				var key2 = arena2.Value.GetValue(off2).blueprintKey;
				storage.Add(arena2, off2, CompiledEnvironment.Compile());

				var id1 = scope.CreateInstance(ref storage, key1);
				var id2 = scope.CreateInstance(ref storage, key2);
				ref var c1 = ref storage.Get(key1);
				c1.GetStaticNodeSlice(0).Cast<StubSeed>().Value() = new StubSeed { output = PortHandle(ref c1, 0, 0), seed = 100L };
				var out1 = PortHandle(ref c1, 0, 0);
				ref var c2 = ref storage.Get(key2);
				c2.GetStaticNodeSlice(0).Cast<StubSeed>().Value() = new StubSeed { output = PortHandle(ref c2, 0, 0), seed = 200L };
				var out2 = PortHandle(ref c2, 0, 0);

				orch.Inject(new[] { Entry(id1, 0), Entry(id2, 0) });
				Assert.AreEqual(2, orch.Run(ref scope, ref storage), "Обе ноды (по одной на блюпринт) исполнены.");

				Assert.IsTrue(scope.GetInstanceCache(id1).Read(out1, out var r1));
				Assert.IsTrue(scope.GetInstanceCache(id2).Read(out2, out var r2));
				Assert.AreEqual(100L, r1, "Инстанс bp1 → 100.");
				Assert.AreEqual(200L, r2, "Инстанс bp2 → 200 (per-instance compiled, независимо).");
			}
			finally
			{
				orch.Dispose();
				scope.Dispose();
				storage.Dispose();
				registry.Dispose();
			}
		}
	}
}
#endif
