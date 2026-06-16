#if UNITY_5_3_OR_NEWER
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
	}
}
