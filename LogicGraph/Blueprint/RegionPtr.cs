using System.Runtime.InteropServices;
using Sapientia.Data;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Указатель карты (Map) на данные одного In/Out — в каком регионе они лежат + как до них добраться.
	/// Порт <b>всегда одного региона</b> ⇒ три представления — <b>union на одном офсете</b> (<c>[FieldOffset(8)]</c>),
	/// дискриминатор — <see cref="region"/>:
	/// <list type="bullet">
	/// <item><b>Static</b> → <see cref="staticData"/> — <b>прямая self-relative ссылка</b> в блоб (<c>GetPtr()</c>);
	/// резолв <b>на месте</b> через ref слота (на копии адрес сломается).</item>
	/// <item><b>Cache</b> → <see cref="cacheData"/> — <b>ordinal ячейки</b> (<see cref="Id{T}"/> по <c>CacheLink</c>) в
	/// <c>InstanceCache</c>. Офсет значения в карте НЕ лежит — он забейкан в шаблоне
	/// <c>CompiledBlueprintHeader.cacheCellsTemplate</c> (по ordinal), который копируется в ячейку (<c>CacheLink.valueOffset</c>).</item>
	/// <item><b>InstancePersistence</b> → <see cref="instanceData"/> — офсет слайса в блоке региона; резолв
	/// <c>база региона + офсет</c> делает владелец Runtime-памяти.</item>
	/// </list>
	/// In пишет указатель на данные своего источника (того же Out); Out — на собственную ячейку.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct RegionPtr
	{
		[FieldOffset(0)]
		public MemoryRegion region;

		/// <summary>Только Static: self-relative ссылка на данные в блобе.</summary>
		[FieldOffset(8)]
		public RelativePtr<byte> staticData;

		/// <summary>Только Cache: ordinal ячейки в <c>InstanceCache</c> (значение — через бейк <c>CacheLink.valueOffset</c>).</summary>
		[FieldOffset(8)]
		public Id<CacheLink> cacheData;

		/// <summary>Только InstancePersistence: офсет слайса ноды в блоке региона.</summary>
		[FieldOffset(8)]
		public PtrOffset instanceData;
	}
}
