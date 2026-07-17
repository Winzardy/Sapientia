#if UNITY_5_4_OR_NEWER
using System;
using System.Reflection;
using NUnit.Framework;
using Sapientia.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// M6-E: <b>version gate</b> — «версия кода» (<see cref="NodeContractHash"/>) — <b>верхнеуровневая сущность
	/// контроля версий группы блюпринтов</b>: живёт в <see cref="CompiledEnvironment"/> (не в блобе). Окружение
	/// компилируется заранее и грузится в рантайме (не рефлексией). <b>Сторедж рождается с окружением</b>; при
	/// <c>Add</c> группе передаётся <b>её</b> окружение, и сторедж сверяет его со своим
	/// (<see cref="CompiledEnvironment.IsCompatibleWith"/>) — несовместимая группа <b>отклоняется</b>. Покрыто
	/// <b>реально</b> (managed-семантика): хеш/предикат — чисто (явные типы); окружение/accept/reject — через
	/// настоящие <c>CompileLayout</c>/<c>Add</c>. <b>Без <see cref="Assert.Ignore(string)"/></b>.
	/// </summary>
	public class VersionGateTests
	{
		// Два различных типа-плейсхолдера (НЕ ILogicNode) — для чувствительности хеша к набору/порядку (имя).
		// FoldExecuteBody на них фолдит стабильный маркер «не logic-нода».
		private struct StubTypeA { public long a; }
		private struct StubTypeB { public long b; }

		// Две logic-ноды с РАЗНЫМИ телами Execute (для чувствительности хеша к содержимому метода, IL-слой).
		private struct NodeBodyEmpty : ILogicNode
		{
			public void Execute(ref NodeContext ctx) { }
		}

		private struct NodeBodyWrite : ILogicNode
		{
			public void Execute(ref NodeContext ctx)
			{
				// Запись в поле ref-параметра — наблюдаемый side-effect, IL не вырезается оптимизатором.
				ctx.nodeId = default;
			}
		}

		// Две logic-ноды с ОДИНАКОВЫМ (пустым) Execute, но РАЗНОЙ раскладкой данных (поля) — для слоя данных.
		private struct NodeDataOne : ILogicNode
		{
			public long a;
			public void Execute(ref NodeContext ctx) { }
		}

		private struct NodeDataTwo : ILogicNode
		{
			public long a;
			public int b; // лишнее поле ⇒ другая раскладка/размер static-слайса
			public void Execute(ref NodeContext ctx) { }
		}

		private static byte[] ExecuteIl(Type type)
		{
			var method = type.GetMethod(nameof(ILogicNode.Execute), BindingFlags.Public | BindingFlags.Instance);
			return method?.GetMethodBody()?.GetILAsByteArray();
		}

		// --- Хеш (чисто) ---

		[Test]
		public void Hash_IsDeterministic()
		{
			// Гард против string.GetHashCode-рандомизации: один и тот же вход даёт один и тот же хеш.
			var first = NodeContractHash.Compute(typeof(StubTypeA), typeof(StubTypeB));
			var second = NodeContractHash.Compute(typeof(StubTypeA), typeof(StubTypeB));
			Assert.AreEqual(first, second, "Хеш одного и того же списка типов детерминирован.");
		}

		[Test]
		public void Hash_OrderSensitive()
		{
			// Ordinal позиционен ⇒ перестановка типов = другая версия кода.
			var ab = NodeContractHash.Compute(typeof(StubTypeA), typeof(StubTypeB));
			var ba = NodeContractHash.Compute(typeof(StubTypeB), typeof(StubTypeA));
			Assert.AreNotEqual(ab, ba, "Перестановка типов меняет хеш (ordinal позиционен).");
		}

		[Test]
		public void Hash_SetSensitive()
		{
			// Добавление типа в таблицу = другая версия кода.
			var a = NodeContractHash.Compute(typeof(StubTypeA));
			var ab = NodeContractHash.Compute(typeof(StubTypeA), typeof(StubTypeB));
			Assert.AreNotEqual(a, ab, "Добавление типа меняет хеш (набор function-table).");
		}

		[Test]
		public void Hash_EmptyIsStableNonZero()
		{
			// Пустая таблица (EditMode): хеш — стабильный seed от FormatVersion, не ноль.
			var first = NodeContractHash.Compute();
			var second = NodeContractHash.Compute(Array.Empty<Type>());
			Assert.AreEqual(first, second, "Пустой вход детерминирован (params [] == Array.Empty).");
			Assert.AreNotEqual(0UL, first, "Seed-хеш пустой таблицы не ноль (свёрнут FormatVersion).");
		}

		// --- Чувствительность к ТЕЛУ метода (IL-слой) ---

		[Test]
		public void Body_DifferentBodies_HaveDifferentIL()
		{
			// Механизм действительно читает тело: IL у двух разных Execute различается.
			var ilEmpty = ExecuteIl(typeof(NodeBodyEmpty));
			var ilWrite = ExecuteIl(typeof(NodeBodyWrite));
			Assert.IsNotNull(ilEmpty, "IL пустого Execute доступен (managed/EditMode).");
			Assert.IsNotNull(ilWrite, "IL непустого Execute доступен.");
			Assert.AreNotEqual(Convert.ToBase64String(ilEmpty), Convert.ToBase64String(ilWrite),
				"Разные тела Execute дают разный IL — иначе хеш тела бессмысленен.");
		}

		[Test]
		public void Hash_BodySensitive()
		{
			// Версия кода чувствительна к телу ноды: разные Execute ⇒ разный хеш.
			Assert.AreNotEqual(NodeContractHash.Compute(typeof(NodeBodyEmpty)), NodeContractHash.Compute(typeof(NodeBodyWrite)),
				"Изменение тела Execute меняет версию кода (IL-слой хеша).");
		}

		[Test]
		public void Hash_BodyDeterministic()
		{
			// Хеш с телом стабилен при повторе (IL читается детерминированно в пределах сборки).
			Assert.AreEqual(NodeContractHash.Compute(typeof(NodeBodyWrite)), NodeContractHash.Compute(typeof(NodeBodyWrite)),
				"Повтор Compute с телом детерминирован.");
		}

		// --- Чувствительность к РАСКЛАДКЕ ДАННЫХ (поля структуры) ---

		[Test]
		public void Hash_DataLayoutSensitive()
		{
			// Одинаковый (пустой) Execute, но разный набор полей ⇒ разная раскладка static-слайса ⇒ разный хеш.
			Assert.AreNotEqual(NodeContractHash.Compute(typeof(NodeDataOne)), NodeContractHash.Compute(typeof(NodeDataTwo)),
				"Смена раскладки данных ноды меняет версию кода (слой данных), даже при одинаковом Execute.");
		}

		// --- Окружение: сборка + предикат совместимости (чисто/реально) ---

		[Test]
		public void Environment_Compile_CapturesLocal()
		{
			// Окружение собирается build-time из текущей function-table.
			Assert.AreEqual(NodeContractHash.Local, CompiledEnvironment.Compile().contractHash,
				"CompiledEnvironment.Compile() фиксирует текущую версию кода.");
		}

		[Test]
		public void Environment_IsCompatibleWith_SameVersion()
		{
			var a = new CompiledEnvironment(0xABCDEF123456UL);
			var b = new CompiledEnvironment(0xABCDEF123456UL);
			Assert.IsTrue(a.IsCompatibleWith(b), "Равные версии кода совместимы.");
		}

		[Test]
		public void Environment_IsCompatibleWith_DifferentVersion()
		{
			var a = new CompiledEnvironment(0xABCDEF123456UL);
			var b = new CompiledEnvironment(0xABCDEF123457UL);
			Assert.IsFalse(a.IsCompatibleWith(b), "Разные версии кода несовместимы.");
		}

		// --- Гейт на storage.Add (реально) ---

		[Test]
		public void Storage_CompatibleGroup_Added()
		{
			// Сторедж и группа несут одно окружение (Compile()) ⇒ группа принимается.
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			try
			{
				var arena = BlueprintCompiler.CompileLayout(StubBlueprint.Of(7, 3, new StubNode(staticSize: 8)), out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile()); // владение ареной → стореджу

				Assert.IsTrue(storage.Has(key), "Совместимая группа добавлена.");
				Assert.AreEqual(1, storage.Count, "Один блюпринт в сторедже.");
			}
			finally
			{
				storage.Dispose();
			}
		}

		[Test]
		public void Storage_StaleGroup_Rejected()
		{
			// Группа собрана под ДРУГУЮ версию кода (имитация рассинхрона окружения) ⇒ Add отклоняет.
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var staleEnvironment = new CompiledEnvironment(unchecked(NodeContractHash.Local + 1));
			try
			{
				var arena = BlueprintCompiler.CompileLayout(StubBlueprint.Of(7, 3, new StubNode(staticSize: 8)), out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;

				Assert.Throws<InvalidOperationException>(() => storage.Add(arena, offset, staleEnvironment),
					"Несовместимая группа отклоняется (throw) — не добавляется.");
				Assert.AreEqual(0, storage.Count, "Отклонённая группа не попала в сторедж (нет полу-состояния).");
				Assert.IsFalse(storage.Has(key), "Отклонённая группа не резолвится.");
			}
			finally
			{
				storage.Dispose();
			}
		}

		// --- Happy-path CreateInstance через совместимый сторедж (интеграция, не гейт) ---

		[Test]
		public void CreateInstance_FromCompatibleStorage_Works()
		{
			var storage = CompiledBlueprintStorage.Create(CompiledEnvironment.Compile());
			var scope = ExecutionScope.Create();
			try
			{
				var arena = BlueprintCompiler.CompileLayout(StubBlueprint.Of(7, 3, new StubNode(staticSize: 8)), out var offset);
				var key = arena.Value.GetValue(offset).blueprintKey;
				storage.Add(arena, offset, CompiledEnvironment.Compile());

				var id = scope.CreateInstance(ref storage, key);
				Assert.IsTrue(scope.Has(id), "Инстанс из совместимого стореджа создаётся, хендл жив.");
			}
			finally
			{
				scope.Dispose();
				storage.Dispose();
			}
		}
	}
}
#endif
