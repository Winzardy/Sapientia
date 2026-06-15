using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Граф связей нод (топология блюпринта) — <b>инстанс-агностичен</b>: целиком определяется связями
	/// (<see cref="Blueprint.inputToOutput"/>), одинаков для всех инстансов и run'ов, поэтому бейкается в
	/// Static-блоб (дедуп вместе с ним). Substrate под батч-шедулинг (по нему строится execution-DAG).
	/// <list type="bullet">
	/// <item><see cref="relatives"/> — на ноду (по <see cref="Id{NodeHeader}"/>): ноды-предшественники/потомки.</item>
	/// <item><see cref="startNodes"/> — ноды без предшественников (<c>inDegree == 0</c>): корни исполнения.</item>
	/// </list>
	/// Self-relative (<see cref="BumpArray{T}"/>) — резолвить только через ref/арена-указатель (на копии адрес сломается).
	/// </summary>
	public struct NodeMapHeader
	{
		/// <summary>На ноду: ноды-предшественники (источники её In'ов) и потомки (читатели её Out'ов).</summary>
		public BumpArray<NodeRelativesHeader> relatives;

		/// <summary>Ноды с <c>inDegree == 0</c> (нет нод-предшественников: вход графа / только константы).</summary>
		public BumpArray<Id<NodeHeader>> startNodes;
	}

	/// <summary>
	/// Ноды-соседи одной ноды в топологии (рёбра — по нодам, не по портам; <b>дедуп по ноде</b>: две связи
	/// в одну ноду-источник дают одно ребро). Константы (precalculated-Out без ноды-владельца) рёбер не создают.
	/// </summary>
	public struct NodeRelativesHeader
	{
		/// <summary>Ноды-предшественники (владельцы Out'ов, питающих In'ы этой ноды).</summary>
		public BumpArray<Id<NodeHeader>> inputs;
		/// <summary>Ноды-потомки (те, чьи In'ы питаются Out'ами этой ноды).</summary>
		public BumpArray<Id<NodeHeader>> outputs;

		/// <summary>Число нод-предшественников (счётчик зависимостей для батч-шедулинга, 4B). Только поле <c>length</c> — безопасно на копии.</summary>
		public readonly int InDegree => inputs.Length;
	}
}
