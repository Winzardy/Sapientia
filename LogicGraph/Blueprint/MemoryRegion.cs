using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Три региона памяти ноды/блюпринта (доменная модель Static/Runtime). Числовое значение = индекс слота
	/// в fixed-буфере <see cref="DataSizes"/> — порядок менять нельзя.
	/// </summary>
	public enum MemoryRegion : byte
	{
		/// <summary>Read-only данные нод; единый экземпляр на приложение (дедуп по версии) — живут в compiled-блобе.</summary>
		Static = 0,
		/// <summary>Runtime.Cache инстанса: данные на время исполнения, сбрасывается перед каждым run'ом (RW).</summary>
		Cache = 1,
		/// <summary>Runtime.InstancePersistence инстанса: постоянные данные нод; сохраняются через State (RW).</summary>
		Persistence = 2,
	}

	/// <summary>
	/// Байтовые размеры всех трёх регионов одной ноды, объявляемые на этапе компиляции (см.
	/// <see cref="INode.DataSizes"/>). Индексируется по <see cref="MemoryRegion"/>. Любой размер может быть 0
	/// — нода вправе не занимать какой-либо регион.
	/// </summary>
	public unsafe struct DataSizes
	{
		public const int Count = 3;
		public const int Alignment = 8;

		private fixed int _sizes[Count];

		public DataSizes(int staticSize, int cacheSize, int persistanceSize)
		{
			_sizes[MemoryRegion.Static.ToInt()] = staticSize;
			_sizes[MemoryRegion.Cache.ToInt()] = cacheSize;
			_sizes[MemoryRegion.Persistence.ToInt()] = persistanceSize;
		}

		public int this[MemoryRegion region]
		{
			get => _sizes[region.ToInt()];
			set => _sizes[region.ToInt()] = value;
		}

		public int this[int region]
		{
			get => _sizes[region];
			set => _sizes[region] = value;
		}

		public int GetAligned(MemoryRegion region)
		{
			return GetAligned(region.ToInt());
		}

		public int GetAligned(int region)
		{
			return _sizes[region].AlignUp(Alignment);
		}

		/// <summary>Копия с каждым размером, выровненным до <see cref="Alignment"/>.</summary>
		public DataSizes GetAligned()
		{
			var result = this;
			for (var l = 0; l < Count; l++)
				result[l] = result[l].AlignUp(Alignment);
			return result;
		}

		/// <summary>Поэлементная сумма размеров по всем регионам.</summary>
		public static DataSizes operator +(DataSizes a, DataSizes b)
		{
			for (var l = 0; l < Count; l++)
				a[l] += b[l];
			return a;
		}
	}
}
