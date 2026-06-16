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
	}
}
