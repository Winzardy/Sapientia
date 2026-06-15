using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Маркер <b>категории</b> ambient-контекста LogicGraph (аналог <c>IWorldService</c> для реестра сервисов).
	/// Конкретные типы контекста реализуют его (через <see cref="IIndexedType"/> регистрируются в
	/// <see cref="IndexedTypes"/> под этой категорией), а нода объявляет нужные ей контексты как
	/// <c>TypeId&lt;INodeContext&gt;</c> (см. <see cref="INode.GetContextTypes"/>). Компилятор собирает union этих
	/// типов (дедуп) в <c>CompiledBlueprintHeader.contextTypes</c> (4E). Реестр-владелец «TypeId → указатель»,
	/// наполняемый при исполнении, — <c>ExecutionScope</c> (4F).
	/// </summary>
	public interface INodeContext : IIndexedType
	{
	}
}
