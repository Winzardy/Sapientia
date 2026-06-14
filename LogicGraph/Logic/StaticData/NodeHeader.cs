using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph
{
	/// <summary>Флаги ноды (комбинируемые). Семантика — на подтверждение (см. ревью).</summary>
	[Flags]
	public enum NodeState : byte
	{
		None = 0,
		HasCache = 1 << 0, // нода производит кешируемый Out (гейт мемоизации Is-Calculated)
		Multiple = 1 << 1, // нода может вызываться несколько раз / в параллельных батчах
	}

	/// <summary>
	/// Заголовок одной ноды в <see cref="CompiledBlueprintHeader"/>: Data (индекс метода + ссылки на слайсы по
	/// регионам) + указатель на блок In/Out ноды.
	/// </summary>
	public struct NodeHeader
	{
		/// <summary>Индекс метода обработки ноды (Static.Data; seed диспатча M6).</summary>
		public TypeId<INode> typeId;
		public RuntimeType runtimeType;

		/// <summary>
		/// Static-слайс ноды — <b>прямая self-relative ссылка</b> на данные в блобе (без общего static-массива).
		/// Резолв — <c>staticData.GetPtr()</c> через ref/арена-указатель (на копии адрес сломается).
		/// </summary>
		public RelativePtr staticData;

		/// <summary>Офсет слайса ноды в блоке Persistence-региона (база — у владельца Runtime-памяти).</summary>
		public PtrOffset persistence;
		/// <summary>
		/// Блок In/Out ноды — <b>массив байт</b> (заполняется на компиляции; во время run'а нода читает его как
		/// свою struct, число полей нода знает сама). Офсет идёт <b>от позиции <see cref="CompiledBlueprintHeader"/></b>
		/// (резолв <c>адрес заголовка + офсет</c>). Один указатель на In и Out вместе.
		/// </summary>
		public PtrOffset inOut;
	}
}
