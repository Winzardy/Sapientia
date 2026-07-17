using Sapientia.Extensions;
using Submodules.Sapientia.Data;
#if UNITY_5_3_OR_NEWER
using Sapientia.Collections;
using Unity.Burst;
#endif

namespace Sapientia.LogicGraph
{
	/// <summary>Ячейка function-table ноды: функция исполнения одной ноды над <see cref="NodeContext"/>. Под Burst
	/// адресуется как <c>FunctionPointer&lt;ExecuteFn&gt;</c> (раздаётся по индексу <c>TypeId&lt;ILogicNode&gt;</c>, M6-C);
	/// в чистом .NET — обычный managed-делегат.</summary>
	public delegate void ExecuteFn(ref NodeContext ctx);

	/// <summary>
	/// <b>Адаптация типа ноды в указатель на функцию</b> (как исторический <c>NodeInvoker.CompileDoNode</c>): generic
	/// <see cref="Execute{T}"/> — единая точка, монолитно компилируемая Burst'ом под каждый конкретный <c>T</c>
	/// (<see cref="Compile{T}"/> → <see cref="ExecuteFn"/>-fn-pointer) либо отдаваемая managed-делегатом (.NET, fallback).
	/// <b>Рантайм-диспатч — по индексу через function pointer, без vtable;</b> вызов <c>body.Execute</c> внутри
	/// монолитной <see cref="Execute{T}"/> на конкретном <c>T</c> девиртуализуется (constrained call), не виртуальный.
	/// Сборка таблицы по индексу (реестр) — M6-C.
	/// </summary>
#if UNITY_5_3_OR_NEWER
	[BurstCompile]
#endif
	public static class NodeInvoker
	{
		/// <summary>Адаптер ноды <typeparamref name="T"/>: резолвит тело из static-слайса и исполняет его. Монолитна
		/// под конкретный <c>T</c> (Burst компилирует каждую инстанцию; constrained-вызов тела, без vtable).</summary>
#if UNITY_5_3_OR_NEWER
		[BurstCompile]
#endif
		public static void Execute<T>(ref NodeContext ctx) where T : unmanaged, ILogicNode
		{
			ref var body = ref ctx.Body<T>();
			body.Execute(ref ctx);
		}

#if UNITY_5_3_OR_NEWER
		/// <summary>Компилирует Burst-fn-pointer адаптера ноды <typeparamref name="T"/>
		/// (<c>CompileFunctionPointer</c> — один раз на тип; реестр по индексу — M6-C). Только под Unity (под Burst).</summary>
		public static FunctionPointer<ExecuteFn> Compile<T>() where T : unmanaged, ILogicNode
		{
			return BurstCompiler.CompileFunctionPointer<ExecuteFn>(Execute<T>);
		}
#endif

		/// <summary>Managed-делегат адаптера ноды <typeparamref name="T"/> (.NET-путь / fallback, когда Burst недоступен).</summary>
		public static ExecuteFn GetManaged<T>() where T : unmanaged, ILogicNode
		{
			return Execute<T>;
		}

		// ─────────────────────────── Per-node диспатч (M6-F) ───────────────────────────
		// Прогон (демандный work-list) — в Orchestrator (M7-B); здесь — чистый диспатч одной ноды:
		// managed-glue Invoke + Burst/Managed таблицы-точки.

#if UNITY_5_3_OR_NEWER
		/// <summary><b>Burst-путь</b> исполнения ноды <paramref name="ordinal"/> над <paramref name="ctx"/>. Принимает
		/// <b>только blittable</b>: off-allocator <paramref name="table"/> (<see cref="NodeFunctionRegistry.BurstTable"/>) +
		/// <see cref="NodeContext"/> — managed-реестр <b>не трогает</b>, поэтому компилируется Burst'ом. Развилки рантайма
		/// внутри нет: бэкенд выбран заранее (граф собран до прогона), здесь только вызов fn-pointer'а.</summary>
		/// <remarks><paramref name="ordinal"/> — по <c>in</c> (readonly-ref): <c>[BurstCompile]</c> entry-point
		/// запрещает структуры <b>по значению</b> в параметрах (BC1064), а <c>in</c> — это указатель (как и
		/// <paramref name="table"/>/<paramref name="ctx"/>), что ABI-легально.</remarks>
		[BurstCompile]
		public static void InvokeBurst(in UnsafeArray<FunctionPointer<ExecuteFn>> table, in Id<ExecuteFn> ordinal, ref NodeContext ctx)
		{
			// Guard (DEBUG): Burst-fn обязан быть скомпилирован, иначе рассинхрон NodeHeader.runtimeType и реестра.
			E.ASSERT(table[ordinal].IsCreated, "[NodeInvoker] Burst-fn не скомпилирован — рассинхрон NodeHeader.runtimeType и реестра.");
			table[ordinal].Invoke(ref ctx);
		}
#endif

		/// <summary><b>Managed-путь</b> исполнения ноды <paramref name="ordinal"/> над <paramref name="ctx"/> (.NET /
		/// fallback / forceManaged). <paramref name="table"/> — <see cref="NodeFunctionRegistry.ManagedTable"/>.</summary>
		public static void InvokeManaged(ExecuteFn[] table, in Id<ExecuteFn> ordinal, ref NodeContext ctx)
		{
			// Guard (DEBUG): делегат обязан быть населён на managed-пути (форс/Managed-нода/.NET).
			E.ASSERT(table[ordinal] != null, "[NodeInvoker] managed-делегат отсутствует на managed-пути (buildManaged:false без форса?).");
			table[ordinal].Invoke(ref ctx);
		}

		/// <summary>
		/// <b>Исполнить одну ноду инстанса</b> (per-node seam, M6-F): собирает <see cref="NodeContext"/> из памяти
		/// инстанса (<see cref="ExecutionScope"/>: Cache + Persistence) и блоба (<paramref name="compiled"/>), читает
		/// заголовок ноды <b>один раз</b> (<see cref="CompiledBlueprintHeader.GetNode"/>: <see cref="NodeHeader.typeId"/>
		/// — ordinal, <see cref="NodeHeader.runtimeType"/> — бэкенд) и зовёт <b>сразу нужную</b> таблицу-специфичную
		/// точку — <see cref="InvokeBurst"/> или <see cref="InvokeManaged"/>.
		/// </summary>
		/// <remarks><b>Managed-glue</b> (не <c>[BurstCompile]</c>): <see cref="ExecutionScope"/> несёт managed-поле
		/// (managed-таблицу реестра) ⇒ сам не Burst-блиттабл. Развилку бэкенда (<see cref="NodeFunctionRegistry.UseManaged"/>)
		/// делает здесь, один раз на ноду — в <see cref="InvokeBurst"/> (Burst-горячий путь) передаётся только
		/// blittable-таблица. <paramref name="compiled"/> берётся <b>по ref</b> (резолв self-relative Static/Map — не на
		/// копии; <c>compiled.AsSafePtr()</c> даёт его адрес).</remarks>
		public static void Invoke(ref ExecutionScope scope, ref CompiledBlueprintHeader compiled, NodeInstanceId node)
		{
			var ctx = new NodeContext
			{
				compiled = compiled.AsSafePtr(),
				cache = scope.GetInstanceCachePtr(node.blueprintId),
				persistence = scope.GetInstancePersistencePtr(node.blueprintId),
				nodeId = node.nodeId,
			};

			ref readonly var header = ref compiled.GetNode(node.nodeId);
			var registry = scope.Registry;
			// typeId (TypeId<ILogicNode>) численно == слот функции в реестре; мост между id-семействами через int
			// (две user-конверсии не цепляются), дальше — типизированный Id<ExecuteFn> (в Invoke* по in, BC1064).
			int typeIndex = header.typeId;
			Id<ExecuteFn> ordinal = typeIndex;
#if UNITY_5_3_OR_NEWER
			if (!registry.UseManaged(header.runtimeType))
			{
				InvokeBurst(registry.BurstTable, ordinal, ref ctx);
				return;
			}
#endif
			InvokeManaged(registry.ManagedTable, ordinal, ref ctx);
		}
	}
}
