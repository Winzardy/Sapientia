#if UNITY_5_4_OR_NEWER
using NUnit.Framework;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M6-A: dispatch-id ноды — плотный ordinal logic-тела в контексте <see cref="ILogicNode"/>
	/// (<c>TypeId&lt;ILogicNode&gt;</c>, через <c>TypeIdOf&lt;ILogicNode, TLogicNode&gt;</c>); адресует
	/// function-table диспатча (M6-C). Здесь — только проводка id: компилятор пишет <c>node.NodeTypeId</c> в
	/// <see cref="NodeHeader.typeId"/>. Проводка проверяется без <see cref="IndexedTypes"/> (id задаётся явно).
	/// Плотность реальных ordinal требует регистрации в <see cref="IndexedTypes"/> — в EditMode <c>Count</c> = 0,
	/// поэтому под <see cref="Assert.Ignore(string)"/> (как 4E/4F-2).
	/// </summary>
	public class DispatchIndexTests
	{
		/// <summary>Минимальное logic-тело для round-trip плотных id (поведения нет; маркер <see cref="ILogicNode"/>).</summary>
		private struct StubLogicA : ILogicNode { }
		private struct StubLogicB : ILogicNode { }

		[Test]
		public void Dispatch_CompilerWritesNodeTypeId()
		{
			// Проводка: id задаётся явно (implicit int→TypeId<ILogicNode>) ⇒ IndexedTypes не нужен.
			var bp = StubBlueprint.Of(
				new StubNode(typeId: 7),
				new StubNode(typeId: 3));

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				int idNode0 = compiled.GetNodeTypeId(0);
				int idNode1 = compiled.GetNodeTypeId(1);
				Assert.AreEqual(7, idNode0, "Компилятор должен пробросить NodeTypeId ноды 0 в NodeHeader.typeId.");
				Assert.AreEqual(3, idNode1, "Компилятор должен пробросить NodeTypeId ноды 1 в NodeHeader.typeId.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Dispatch_DefaultNodeTypeIdIsZero()
		{
			// Дефолтный stub (id не задан) ⇒ ordinal 0 (корректный «нулевой» индекс таблицы).
			var bp = StubBlueprint.Of(new StubNode());

			var arena = BlueprintCompiler.CompileLayout(bp, out var offset);
			try
			{
				ref var compiled = ref arena.Value.GetValue(offset);

				int id = compiled.GetNodeTypeId(0);
				Assert.AreEqual(0, id, "Не заданный NodeTypeId — ordinal 0.");
			}
			finally
			{
				arena.Dispose();
			}
		}

		[Test]
		public void Dispatch_LogicTypesGetDenseIds()
		{
			// Реальные плотные ordinal через TypeIdOf<ILogicNode, T> требуют инициализации IndexedTypes
			// (генератор регистрирует детей контекста ILogicNode). В EditMode Count = 0 ⇒ отложено.
			if (TypeId<ILogicNode>.Count == 0)
			{
				Assert.Ignore("ILogicNode-типы не зарегистрированы в EditMode (IndexedTypes не инициализирован) — плотные id отложены.");
				return;
			}

			int a = TypeIdOf<ILogicNode, StubLogicA>.typeId;
			int b = TypeIdOf<ILogicNode, StubLogicB>.typeId;
			Assert.AreNotEqual(a, b, "Разные logic-типы — разные ordinal.");
			Assert.Less(a, TypeId<ILogicNode>.Count, "ordinal плотный: < Count.");
			Assert.Less(b, TypeId<ILogicNode>.Count, "ordinal плотный: < Count.");
		}
	}
}
#endif
