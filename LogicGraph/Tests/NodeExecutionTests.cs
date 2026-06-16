#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M6-B: контракт исполнения ноды — <see cref="NodeContext"/> + адаптер <see cref="NodeInvoker.Execute{T}"/>
	/// (под Burst → <c>FunctionPointer</c>; рантайм-диспатч по индексу, без vtable). Проверяем <b>managed-путь</b>
	/// (plain .NET, без Burst/IndexedTypes): <c>NodeInvoker.Execute&lt;T&gt;</c> резолвит тело из static-слайса
	/// (<c>NodeContext.Body&lt;T&gt;</c>) и исполняет <c>ILogicNode.Execute</c>; <see cref="NodeContext"/> резолвит
	/// static/cache инстанса. Burst-компиляция (<c>Compile&lt;T&gt;</c>) + реестр по индексу — M6-C. Авто-бейк
	/// port-handle'ов в тело (из Map) отложен (M9) — здесь handle'ы и тело собираем вручную.
	/// </summary>
	public class NodeExecutionTests
	{
		/// <summary>Хендл ячейки <paramref name="index"/> (как в CacheTests): офсет = index*sizeof(CacheLink).</summary>
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

		/// <summary>Тестовое тело-данные ноды: handle'ы In/Out + addend. <see cref="Execute"/> читает In из Cache,
		/// пишет In + addend в Out (резолв тела делает адаптер <c>NodeInvoker.Execute&lt;T&gt;</c>). Handle'ы — вручную.</summary>
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

		[Test]
		public void Exec_NodeBodyExecutesThroughSeam()
		{
			// Узел с достаточным static-слайсом под тело StubAdd (порты блоба не нужны — Cache ручной).
			var bp = StubBlueprint.Of(new StubNode(staticSize: 128));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			var cacheArr = new UnsafeArray<InstanceCache>(default, 1);
			try
			{
				var compiledPtr = arena.Value.GetPtr(offset);

				// Тело в static-слайс (резолв через ref блоба; авто-бейк handle'ов — M9, здесь вручную).
				compiledPtr.Value().GetStaticNodeSlice(0).Cast<StubAdd>().Value() = new StubAdd
				{
					input = H(0),
					output = H(1),
					addend = 100L,
				};

				cacheArr[0] = CreateCache(2);
				cacheArr[0].Write(H(0), 5L); // pre-seed входной ячейки

				var ctx = new NodeContext
				{
					compiled = compiledPtr,
					cache = cacheArr.ptr,
					nodeId = 0,
				};
				// Адаптер ноды (managed-путь Execute<T>; под Burst — тот же код в FunctionPointer, M6-C).
				NodeInvoker.Execute<StubAdd>(ref ctx);

				Assert.IsTrue(cacheArr[0].Read(H(1), out var result), "Out должен быть посчитан после Execute.");
				Assert.AreEqual(105L, result, "Execute читает In (5) и пишет In + addend (100) в Out.");
			}
			finally
			{
				cacheArr[0].Dispose();
				cacheArr.Dispose();
				arena.Dispose();
			}
		}

		[Test]
		public void Exec_NodeContextResolvesStaticAndCache()
		{
			// Узел: static-слайс + один Cache-Out (⇒ cacheCellCount=1, ordinal 0). cacheSize >= sizeof(CacheLink)=16
			// (иначе lockstep-бюджет Cache в BlueprintCompiler не сойдётся).
			var bp = StubBlueprint.Of(new StubNode(staticSize: 32, cacheSize: 16, outputs: new NodeOutput[] { new NodeOutput<long>() }));
			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			var cacheArr = new UnsafeArray<InstanceCache>(default, 1);
			try
			{
				var compiledPtr = arena.Value.GetPtr(offset);
				ref var compiled = ref compiledPtr.Value();
				cacheArr[0] = InstanceCache.Create(default, compiled.cacheCellCount, compiled.cacheValuesSize, compiled.GetCacheCellsTemplate());

				var ctx = new NodeContext
				{
					compiled = compiledPtr,
					cache = cacheArr.ptr,
					nodeId = 0,
				};

				// StaticSlice резолвит записываемую память слайса ноды.
				ctx.StaticSlice().Cast<long>().Value() = 123L;
				Assert.AreEqual(123L, ctx.StaticSlice().Cast<long>().Value(), "StaticSlice резолвит слайс ноды в блобе.");

				// Cache() резолвит InstanceCache инстанса (ordinal 0).
				ctx.Cache().Write(H(0), 77L);
				Assert.IsTrue(ctx.Cache().Read(H(0), out var v));
				Assert.AreEqual(77L, v, "Cache() резолвит InstanceCache инстанса.");
			}
			finally
			{
				cacheArr[0].Dispose();
				cacheArr.Dispose();
				arena.Dispose();
			}
		}
	}
}
#endif
