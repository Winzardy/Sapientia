namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Бэкенд исполнения ноды (форк 8): пишется из <c>INode.RuntimeType</c> в <see cref="NodeHeader.runtimeType"/>,
	/// выбор таблицы диспатча — <see cref="NodeFunctionRegistry.UseManaged"/>. Граница Burst↔managed как wave —
	/// M7-C (нода другого рантайма откладывается на следующий wave).
	/// </summary>
	public enum RuntimeType : byte
	{
		Unmanaged,
		Managed,
	}
}
