#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.Data;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Фаза 4: <see cref="BlueprintInstanceHeader"/> — <b>чистая рантайм-сущность</b> (identity + абстрактные офсеты
	/// <see cref="PtrOffset"/>), без памяти/<c>WorldState</c>/Mem-сущностей. Проверяет, что <c>Create</c> переносит
	/// identity из <c>CompiledBlueprintHeader</c> и сохраняет офсеты. Аллокация/reset/доступ к памяти — у
	/// <see cref="ExecutionScope"/> (см. ExecutionScopeTests).
	/// </summary>
	public class InstanceScopeTests
	{
		[Test]
		public void Instance_CreateWiresIdentityAndOffsets()
		{
			var arena = BlueprintCompiler.CompileLayout(StubBlueprint.Of(7, 3, new StubNode(cacheSize: 16)), out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);
				var instance = BlueprintInstanceHeader.Create(compiled, new PtrOffset(32), default);

				Assert.AreEqual((Id<Blueprint>)7, instance.blueprintId.Id, "blueprintId не из compiled.");
				Assert.AreEqual(3, (int)instance.blueprintId.Version, "version не из compiled.");
				Assert.IsTrue(instance.instanceCache.isValid, "Валидный cache-офсет должен сохраниться.");
				Assert.AreEqual(32, instance.instanceCache.byteOffset, "Неверный cache-офсет.");
				Assert.IsFalse(instance.instancePersistent.isValid, "default персистент-офсет — невалиден (zero-size).");
			}
			finally
			{
				arena.Dispose();
			}
		}
	}
}
#endif
