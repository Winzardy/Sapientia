#if UNITY_5_4_OR_NEWER
using System;
using NUnit.Framework;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M6-D: <b>выбор бэкенда</b> (<see cref="NodeFunctionRegistry.UseManaged"/> по <c>NodeHeader.runtimeType</c> +
	/// глобальный managed-форс) и <b>реальное исполнение managed-путём</b> (<see cref="NodeFunctionRegistry.Invoke"/>,
	/// без Burst/IndexedTypes). Managed-путь исполняется по-настоящему даже в EditMode (plain managed-семантика) ⇒
	/// selection/исполнение/детерминизм покрыты реально; фактический Burst-прогон и reflection-<c>Build</c> требуют
	/// инициализированного <see cref="IndexedTypes"/> ⇒ под <see cref="Assert.Ignore(string)"/>. «Детерминизм
	/// Burst↔.NET» в части Burst — конструктивный аргумент (единый исходник <c>NodeInvoker.Execute&lt;T&gt;</c>
	/// компилируется в обе таблицы), не runtime-assert.
	/// </summary>
	public class BackendSelectionTests
	{
		/// <summary>Хендл ячейки <paramref name="index"/> (как в CacheTests/NodeFunctionRegistryTests).</summary>
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

		/// <summary>Unmanaged-тело (дефолт <see cref="RuntimeType.Unmanaged"/>): In + addend → Out.</summary>
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

		/// <summary>Managed-тело (объявляет <see cref="RuntimeType.Managed"/>): та же логика, но реестр НЕ компилит для
		/// него Burst, а диспатч выбирает managed-таблицу.</summary>
		private struct StubManagedAdd : ILogicNode
		{
			public CacheHandler<long> input;
			public CacheHandler<long> output;
			public long addend;

			public RuntimeType RuntimeType => RuntimeType.Managed;

			public void Execute(ref NodeContext ctx)
			{
				ref var cache = ref ctx.Cache();
				cache.Read(input, out var v);
				cache.Write(output, v + addend);
			}
		}

		/// <summary>Тело с float-математикой (детерминизм managed-пути): пишет фикс-биты результата в Out.</summary>
		private struct StubFloat : ILogicNode
		{
			public CacheHandler<long> input;
			public CacheHandler<long> output;

			public void Execute(ref NodeContext ctx)
			{
				ref var cache = ref ctx.Cache();
				cache.Read(input, out var v);
				var f = v * 0.1f + 3.14159f;
				cache.Write(output, (long)(f * 1_000_000f));
			}
		}

		/// <summary>Generic-нода: проверяет, что <c>INode&lt;T&gt;.RuntimeType</c> выводится из logic-типа.</summary>
		private sealed class GenNode<T> : INode<T> where T : unmanaged, ILogicNode
		{
			public NodeInput[] GetInputs() => Array.Empty<NodeInput>();
			public NodeOutput[] GetOutputs() => Array.Empty<NodeOutput>();
		}

		// --- Selection (UseManaged) ---

		[Test]
		public void Select_ManagedRuntimeType_UsesManaged()
		{
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubManagedAdd>() });
			try
			{
				Assert.IsTrue(registry.UseManaged(RuntimeType.Managed), "Managed-нода всегда идёт managed-путём.");
			}
			finally
			{
				registry.Dispose();
			}
		}

		[Test]
		public void Select_UnmanagedRuntimeType_PerEnv()
		{
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubAdd>() });
			try
			{
#if UNITY_5_3_OR_NEWER
				Assert.IsFalse(registry.UseManaged(RuntimeType.Unmanaged), "Под Unity Unmanaged-нода (без форса) → Burst.");
#else
				Assert.IsTrue(registry.UseManaged(RuntimeType.Unmanaged), "В чистом .NET Burst недоступен → всегда managed.");
#endif
			}
			finally
			{
				registry.Dispose();
			}
		}

		[Test]
		public void Select_ForceManaged_AlwaysManaged()
		{
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubAdd>() }, forceManaged: true);
			try
			{
				Assert.IsTrue(registry.ForceManaged, "Форс прокинут в инстанс.");
				Assert.IsTrue(registry.UseManaged(RuntimeType.Unmanaged), "Форс гонит даже Unmanaged-ноду managed-путём.");
				Assert.IsTrue(registry.UseManaged(RuntimeType.Managed));
			}
			finally
			{
				registry.Dispose();
			}
		}

		// --- Реальное managed-исполнение через Invoke ---

		[Test]
		public void Invoke_ManagedNode_ExecutesManaged()
		{
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubManagedAdd>() });
			var bp = StubBlueprint.Of(new StubNode(staticSize: 128));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			var cacheArr = new UnsafeArray<InstanceCache>(default, 1);
			try
			{
				var compiledPtr = arena.Value.GetPtr(offset);
				compiledPtr.Value().GetStaticNodeSlice(0).Cast<StubManagedAdd>().Value() = new StubManagedAdd
				{
					input = H(0),
					output = H(1),
					addend = 100L,
				};

				cacheArr[0] = CreateCache(2);
				cacheArr[0].Write(H(0), 5L);

				var ctx = new NodeContext { compiled = compiledPtr, cache = cacheArr.ptr, nodeId = 0 };
				// Managed runtimeType ⇒ Invoke выбирает managed-таблицу и реально исполняет тело.
				registry.Invoke(0, RuntimeType.Managed, ref ctx);

				Assert.IsTrue(cacheArr[0].Read(H(1), out var result), "Out посчитан managed-путём.");
				Assert.AreEqual(105L, result, "5 + 100 через выбранную managed-таблицу.");
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
		public void Invoke_ForceManaged_RunsUnmanagedNodeManaged()
		{
			// Unmanaged-нода, но форс ⇒ исполняется managed-путём (реально), без обращения к Burst-таблице.
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubAdd>() }, forceManaged: true);
			var bp = StubBlueprint.Of(new StubNode(staticSize: 128));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			var cacheArr = new UnsafeArray<InstanceCache>(default, 1);
			try
			{
				var compiledPtr = arena.Value.GetPtr(offset);
				compiledPtr.Value().GetStaticNodeSlice(0).Cast<StubAdd>().Value() = new StubAdd
				{
					input = H(0),
					output = H(1),
					addend = 100L,
				};

				cacheArr[0] = CreateCache(2);
				cacheArr[0].Write(H(0), 5L);

				var ctx = new NodeContext { compiled = compiledPtr, cache = cacheArr.ptr, nodeId = 0 };
				registry.Invoke(0, RuntimeType.Unmanaged, ref ctx);

				Assert.IsTrue(cacheArr[0].Read(H(1), out var result), "Форс направил Unmanaged-ноду в managed.");
				Assert.AreEqual(105L, result);
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
		public void Invoke_Managed_DeterministicAcrossRuns()
		{
			// Детерминизм managed-пути: повтор + разные инстансы дают побитово равный результат (нет Random/wall-clock).
			var registry = NodeFunctionRegistry.Create(new[] { NodeInvoker.GetManaged<StubFloat>() }, forceManaged: true);
			var bp = StubBlueprint.Of(new StubNode(staticSize: 64));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			var cacheA = new UnsafeArray<InstanceCache>(default, 1);
			var cacheB = new UnsafeArray<InstanceCache>(default, 1);
			try
			{
				var compiledPtr = arena.Value.GetPtr(offset);
				compiledPtr.Value().GetStaticNodeSlice(0).Cast<StubFloat>().Value() = new StubFloat
				{
					input = H(0),
					output = H(1),
				};

				cacheA[0] = CreateCache(2);
				cacheA[0].Write(H(0), 42L);
				var ctxA = new NodeContext { compiled = compiledPtr, cache = cacheA.ptr, nodeId = 0 };

				registry.Invoke(0, RuntimeType.Unmanaged, ref ctxA);
				cacheA[0].Read(H(1), out var first);
				// Повторный прогон того же инстанса (ячейка перезаписывается) → тот же результат.
				registry.Invoke(0, RuntimeType.Unmanaged, ref ctxA);
				cacheA[0].Read(H(1), out var second);
				Assert.AreEqual(first, second, "Повтор Invoke детерминирован (побитово).");

				// Второй инстанс с тем же входом → тот же результат.
				cacheB[0] = CreateCache(2);
				cacheB[0].Write(H(0), 42L);
				var ctxB = new NodeContext { compiled = compiledPtr, cache = cacheB.ptr, nodeId = 0 };
				registry.Invoke(0, RuntimeType.Unmanaged, ref ctxB);
				cacheB[0].Read(H(1), out var other);
				Assert.AreEqual(first, other, "Два инстанса с равным входом дают равный результат.");
			}
			finally
			{
				cacheA[0].Dispose();
				cacheB[0].Dispose();
				cacheA.Dispose();
				cacheB.Dispose();
				arena.Dispose();
				registry.Dispose();
			}
		}

		// --- RuntimeType как capability logic-типа + деривация INode<T> ---

		[Test]
		public void RuntimeType_LogicTypeCapability()
		{
			Assert.AreEqual(RuntimeType.Managed, ((ILogicNode)default(StubManagedAdd)).RuntimeType,
				"Managed-тело объявляет capability Managed.");
			Assert.AreEqual(RuntimeType.Unmanaged, ((ILogicNode)default(StubAdd)).RuntimeType,
				"Тело без переопределения берёт DIM-дефолт Unmanaged.");
		}

		[Test]
		public void RuntimeType_DerivedByGenericNode()
		{
			Assert.AreEqual(RuntimeType.Managed, ((INode)new GenNode<StubManagedAdd>()).RuntimeType,
				"INode<T>.RuntimeType выводится из logic-типа (Managed).");
			Assert.AreEqual(RuntimeType.Unmanaged, ((INode)new GenNode<StubAdd>()).RuntimeType,
				"INode<T>.RuntimeType выводится из logic-типа (Unmanaged).");
		}

		// --- Build-skip (требует init IndexedTypes) ---

		[Test]
		public void Build_SkipsBurstForManaged()
		{
			// Burst-skip и reflection-Build требуют инициализированного IndexedTypes (дети ILogicNode). В EditMode
			// TypeId<ILogicNode>.Count = 0 ⇒ отложено. Managed-путь selection/Invoke покрыт реально через Create.
			if (TypeId<ILogicNode>.Count == 0)
			{
				Assert.Ignore("ILogicNode-типы не зарегистрированы в EditMode (IndexedTypes не инициализирован) — Build-skip отложен.");
				return;
			}

			var registry = NodeFunctionRegistry.Build();
			try
			{
				Assert.IsTrue(registry.IsCreated);
				// Managed-ноды обязаны иметь managed-делегат (единственный путь); Burst для них не компилировался.
				for (var ordinal = 0; ordinal < TypeId<ILogicNode>.Count; ordinal++)
				{
					var type = IndexedTypes.GetType(IndexedTypes.GetContextChildren(typeof(ILogicNode))[ordinal]);
					var isManaged = ((ILogicNode)Activator.CreateInstance(type)).RuntimeType == RuntimeType.Managed;
					if (isManaged)
						Assert.IsNotNull(registry.GetManaged(ordinal), $"Managed-нода ordinal {ordinal} обязана иметь managed-делегат.");
				}
			}
			finally
			{
				registry.Dispose();
			}
		}
	}
}
#endif
