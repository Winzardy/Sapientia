#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// 4F-2: <see cref="ExecutionScope"/> — wiring ambient-context-реестра (<see cref="ContextRegistry{TContext}"/>) на scope:
	/// create/dispose + generic-API set/get/has через scope. Память/lifecycle инстансов — 4F-1 (<c>InstanceScopeTests</c>);
	/// здесь — только context-реестр. Round-trip — под <see cref="Assert.Ignore(string)"/> (в EditMode типы не зарегистрированы).
	/// </summary>
	public class ExecutionScopeTests
	{
		[Test]
		public void Scope_CreateDisposeSmoke()
		{
			var scope = ExecutionScope.Create(default, 8);
			Assert.IsTrue(scope.IsCreated, "Scope создан.");

			scope.Dispose();
			Assert.IsFalse(scope.IsCreated, "После Dispose scope невалиден (реестр контекста освобождён).");
			scope.Dispose(); // идемпотентно
		}

		[Test]
		public void Scope_ContextRoundtripWhenRegistered()
		{
			var scope = ExecutionScope.Create(default, 8);
			try
			{
				if (TypeId<INodeContext>.Count == 0)
				{
					Assert.Ignore("INodeContext-типы не зарегистрированы в EditMode — round-trip через scope отложен.");
					return;
				}

				scope.SetContext(new StubContext { value = 99L });
				Assert.IsTrue(scope.HasContext<StubContext>(), "Контекст задан через scope.");
				Assert.AreEqual(99L, scope.GetContext<StubContext>().value, "GetContext через scope должен вернуть значение.");
			}
			finally
			{
				scope.Dispose();
			}
		}
	}
}
#endif
