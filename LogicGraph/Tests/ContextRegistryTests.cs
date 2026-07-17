#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// 4F-2: <see cref="ContextRegistry{TContext}"/> — ambient-context-реестр scope'а (generic-API <c>SetContext&lt;T&gt;</c>/
	/// <c>GetContext&lt;T&gt;</c>; реестр сам владеет блоками контекстов, размер слотов — <c>TypeId&lt;TContext&gt;.Count</c>).
	/// Round-trip требует зарегистрированного в <see cref="IndexedTypes"/> типа контекста — в EditMode <c>Count</c> = 0,
	/// поэтому идёт под <see cref="Assert.Ignore(string)"/> (отложено). Lifecycle/empty — прогоняются.
	/// </summary>
	public class ContextRegistryTests
	{
		[Test]
		public void Context_RoundtripWhenRegistered()
		{
			var reg = ContextRegistry<INodeContext>.Create(default);
			try
			{
				if (!reg.IsCreated)
				{
					Assert.Ignore("INodeContext-типы не зарегистрированы в EditMode (IndexedTypes не инициализирован) — round-trip отложен.");
					return;
				}

				reg.SetContext(new StubContext { value = 1234L });
				Assert.IsTrue(reg.HasContext<StubContext>(), "После SetContext контекст задан.");
				Assert.AreEqual(1234L, reg.GetContext<StubContext>().value, "GetContext должен вернуть записанное значение.");
			}
			finally
			{
				reg.Dispose();
			}
		}

		[Test]
		public void Context_EmptyRegistryAndDispose()
		{
			// В EditMode TypeId<INodeContext>.Count = 0 ⇒ реестр пуст: Has — false, Dispose — no-op и идемпотентен.
			var reg = ContextRegistry<INodeContext>.Create(default);
			Assert.IsFalse(reg.HasContext<StubContext>(), "Незаданный/незарегистрированный контекст — не задан.");
			reg.Dispose();
			reg.Dispose(); // идемпотентно
		}
	}
}
#endif
