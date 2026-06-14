using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Маркер unmanaged-тела ноды (logic-сторона). Старые DoBurst-овэрлоды снесены вместе с edge-моделью —
	/// диспатч/исполнение пересобираются на Static.Map (региональные указатели) в M6.
	/// </summary>
	public interface ILogicNode : IIndexedType
	{
	}
}
