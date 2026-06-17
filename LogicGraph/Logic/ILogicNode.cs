using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Unmanaged-тело ноды (logic-сторона): <b>данные</b> ноды (живут в static-слайсе) + логика <see cref="Execute"/>.
	/// Резолвится из static-слайса (<c>NodeContext.Body&lt;T&gt;</c>) и исполняется через адаптер
	/// <see cref="NodeInvoker.Execute{T}"/>, который под Burst раздаётся как <c>FunctionPointer</c> по индексу
	/// <c>TypeId&lt;ILogicNode&gt;</c> (M6-A/C). <b>Это не виртуальный вызов в рантайме:</b> диспатч идёт по
	/// function-pointer-индексу, а сам <see cref="Execute"/> монолитно компилируется под конкретный тип (constrained
	/// call, без vtable). <c>IIndexedType</c> — чтобы тип был индексируемым контекстом для dispatch-id.
	/// </summary>
	public interface ILogicNode : IIndexedType
	{
		/// <summary>Логика ноды: читает In/Out (через <c>NodeContext.Cache()</c>) + static/persistence и считает ноду.</summary>
		void Execute(ref NodeContext ctx);

		/// <summary>
		/// <b>Capability бэкенда logic-тела</b> (M6-D): может ли тело быть Burst-компилировано. Дефолт —
		/// <see cref="RuntimeType.Unmanaged"/> (Burst-совместимо). Тело, которое лезет в managed-код, объявляет
		/// <see cref="RuntimeType.Managed"/> ⇒ реестр (<see cref="NodeFunctionRegistry.Build"/>) <b>не компилит</b>
		/// для него Burst-fn-pointer (<c>CompileFunctionPointer</c> на managed-теле упал бы), держит только managed-делегат.
		/// Читается <b>только на компиляции/сборке</b> (managed-путь), никогда не на Burst-горячем пути. Per-node
		/// <see cref="NodeHeader.runtimeType"/> выводится из этой capability через <c>INode&lt;TLogicNode&gt;</c> ⇒
		/// совпадает с тем, по чему реестр решал Burst-skip (иначе диспатч ушёл бы в пустой <c>FunctionPointer</c>).
		/// </summary>
		RuntimeType RuntimeType => RuntimeType.Unmanaged;
	}
}
