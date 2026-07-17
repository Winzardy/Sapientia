using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	/// <summary>Флаги ноды для битовой маски <see cref="ByteEnumMask{T}"/>: <b>члены — индексы бит</b> (0,1,…),
	/// не степени двойки; «пусто» — это <c>default</c>/<see cref="ByteEnumMask{T}.HasNothing"/>, отдельного
	/// <c>None</c> нет. Семантика подтверждена (форк 7); выставляются в <see cref="BlueprintCompiler"/> на компиляции.</summary>
	public enum NodeState : byte
	{
		HasCache = 0, // у ноды есть хотя бы один Out в Cache-регионе → результат мемоизируется (гейт Is-Calculated, M8)
		Multiple = 1, // Out'ы ноды читают ≥2 потребителя (fan-out, relatives.outputs.Length > 1): ячейка живёт до прочтения всеми
	}

	/// <summary>
	/// Заголовок одной ноды в <see cref="CompiledBlueprintHeader"/>: Data (индекс метода + бэкенд + флаги +
	/// ссылки на слайсы по регионам) + указатель на блок In/Out ноды.
	/// </summary>
	public struct NodeHeader
	{
		/// <summary>Индекс метода обработки ноды (Static.Data) — плотный ordinal logic-тела в контексте
		/// <see cref="ILogicNode"/> (<c>TypeIdOf&lt;ILogicNode, TLogicNode&gt;</c>). Адресует function-table
		/// диспатча по индексу (M6-C); валиден в пределах «версии кода» (version gate, M6-E).</summary>
		public TypeId<ILogicNode> typeId;
		/// <summary>Бэкенд исполнения (форк 8): пишется из <c>INode.RuntimeType</c>; бакетинг батчей по нему — M7.</summary>
		public RuntimeType runtimeType;
		/// <summary>Флаги ноды (форк 7) битовой маской: <see cref="NodeState.HasCache"/>/<see cref="NodeState.Multiple"/>;
		/// выставляются на компиляции по региону Out'ов / fan-out топологии. Чтение — <c>state.Has(NodeState.X)</c>.</summary>
		public ByteEnumMask<NodeState> state;

		/// <summary>
		/// Static-слайс ноды — <b>прямая self-relative ссылка</b> на данные в блобе (без общего static-массива).
		/// Резолв — <c>staticData.GetPtr()</c> через ref/арена-указатель (на копии адрес сломается).
		/// </summary>
		public RelativePtr staticData;

		/// <summary>Офсет слайса ноды в блоке InstancePersistence-региона (база — у владельца Runtime-памяти).</summary>
		public PtrOffset persistence;
		/// <summary>
		/// Блок In/Out ноды — <b>массив байт</b> (заполняется на компиляции; во время run'а нода читает его как
		/// свою struct, число полей нода знает сама). Офсет идёт <b>от позиции <see cref="CompiledBlueprintHeader"/></b>
		/// (резолв <c>адрес заголовка + офсет</c>). Один указатель на In и Out вместе.
		/// </summary>
		public PtrOffset inOut;
	}
}
